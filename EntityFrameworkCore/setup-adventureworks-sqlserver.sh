# Function to parse command line arguments
parse_args() {
    if [ -z "$1" ]; then
        print_error "No backup file specified. Usage: ./setup-adventureworks-sqlserver.sh <backup_file>"
        exit 1
    fi

    BACKUP_FILE="$1"
    DATABASE_NAME="$(basename \"$BACKUP_FILE\" .bak)"
}

# Add parse_args to main function call
parse_args "$@"

#!/bin/bash

# AdventureWorksLT2022 SQL Server Setup Script
# This script creates a SQL Server Docker container and restores the AdventureWorksLT2022 backup

set -e  # Exit on any error

# Configuration variables
CONTAINER_NAME="sqlpreview"
SA_PASSWORD="MySaPassword123"
BACKUP_FILE=""  # Will be set from command line argument
DATABASE_NAME=""  # Will be derived from backup file name
SQL_PORT="1433"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if Docker is installed and running
check_docker() {
    print_info "Checking Docker installation..."
    
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed. Please install Docker and try again."
        exit 1
    fi
    
    if ! docker info &> /dev/null; then
        print_error "Docker is not running. Please start Docker and try again."
        exit 1
    fi
    
    print_success "Docker is installed and running"
}

# Function to check if backup file exists
check_backup_file() {
    print_info "Checking for backup file: $BACKUP_FILE"
    
    if [ ! -f "$BACKUP_FILE" ]; then
        print_error "Backup file '$BACKUP_FILE' not found in current directory."
        print_info "Please ensure the AdventureWorksLT2022.bak file is in the same directory as this script."
        exit 1
    fi
    
    print_success "Backup file found: $BACKUP_FILE ($(du -h "$BACKUP_FILE" | cut -f1))"
}

# Function to stop and remove existing container if it exists
cleanup_existing_container() {
    print_info "Checking for existing container: $CONTAINER_NAME"
    
    if docker ps -a --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
        print_warning "Container '$CONTAINER_NAME' already exists. Removing it..."
        docker stop "$CONTAINER_NAME" &> /dev/null || true
        docker rm "$CONTAINER_NAME" &> /dev/null || true
        print_success "Existing container removed"
    fi
}

# Function to create SQL Server container
create_container() {
    print_info "Creating SQL Server 2022 container: $CONTAINER_NAME"
    
    docker run \
        --restart unless-stopped \
        -e "ACCEPT_EULA=Y" \
        -e "MSSQL_SA_PASSWORD=$SA_PASSWORD" \
        -e "MSSQL_PID=Evaluation" \
        -p "$SQL_PORT:1433" \
        --name "$CONTAINER_NAME" \
        --hostname "$CONTAINER_NAME" \
        -d mcr.microsoft.com/mssql/server:2022-latest
    
    print_success "Container created successfully"
}

# Function to wait for SQL Server to be ready
wait_for_sql_server() {
    print_info "Waiting for SQL Server to be ready..."
    
    local max_attempts=30
    local attempt=1
    
    while [ $attempt -le $max_attempts ]; do
        if docker exec "$CONTAINER_NAME" /opt/mssql-tools18/bin/sqlcmd \
            -S localhost -U sa -P "$SA_PASSWORD" -C \
            -Q "SELECT 1" &> /dev/null; then
            print_success "SQL Server is ready!"
            return 0
        fi
        
        print_info "Attempt $attempt/$max_attempts - SQL Server not ready yet, waiting 5 seconds..."
        sleep 5
        ((attempt++))
    done
    
    print_error "SQL Server failed to start within expected time"
    exit 1
}

# Function to copy backup file to container
copy_backup_file() {
    print_info "Copying backup file to container..."
    
    docker cp "$BACKUP_FILE" "$CONTAINER_NAME:/var/opt/mssql/data/"
    
    print_success "Backup file copied to container"
}

# Function to get logical file names from backup
get_logical_names() {
    print_info "Reading backup file information..."
    
    # Get logical file names and store them
    local filelistonly_output
    filelistonly_output=$(docker exec "$CONTAINER_NAME" /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "$SA_PASSWORD" -C \
        -Q "RESTORE FILELISTONLY FROM DISK = '/var/opt/mssql/data/$BACKUP_FILE'" \
        -h -1 -W | grep -E "^[A-Za-z]")
    
    # Extract logical names (first column)
    DATA_LOGICAL_NAME=$(echo "$filelistonly_output" | head -1 | awk '{print $1}')
    LOG_LOGICAL_NAME=$(echo "$filelistonly_output" | tail -1 | awk '{print $1}')
    
    print_info "Data file logical name: $DATA_LOGICAL_NAME"
    print_info "Log file logical name: $LOG_LOGICAL_NAME"
}

# Function to restore database
restore_database() {
    print_info "Restoring database: $DATABASE_NAME"
    
    local restore_output
    restore_output=$(docker exec "$CONTAINER_NAME" /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "$SA_PASSWORD" -C \
        -Q "RESTORE DATABASE [$DATABASE_NAME] FROM DISK = '/var/opt/mssql/data/$BACKUP_FILE' 
            WITH MOVE '$DATA_LOGICAL_NAME' TO '/var/opt/mssql/data/${DATABASE_NAME}.mdf', 
                 MOVE '$LOG_LOGICAL_NAME' TO '/var/opt/mssql/data/${DATABASE_NAME}_Log.ldf', 
                 REPLACE")
    
    echo "$restore_output"
    
    if echo "$restore_output" | grep -q "RESTORE DATABASE successfully processed"; then
        print_success "Database restored successfully!"
    else
        print_error "Database restoration failed"
        exit 1
    fi
}

# Function to verify database
verify_database() {
    print_info "Verifying database restoration..."
    
    # Check if database exists
    local db_check
    db_check=$(docker exec "$CONTAINER_NAME" /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "$SA_PASSWORD" -C \
        -Q "SELECT name FROM sys.databases WHERE name = '$DATABASE_NAME'" \
        -h -1 -W)
    
    if echo "$db_check" | grep -q "$DATABASE_NAME"; then
        print_success "Database '$DATABASE_NAME' is available"
        
        # Get table count
        local table_count
        table_count=$(docker exec "$CONTAINER_NAME" /opt/mssql-tools18/bin/sqlcmd \
            -S localhost -U sa -P "$SA_PASSWORD" -C \
            -Q "USE [$DATABASE_NAME]; SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'" \
            -h -1 -W | tr -d ' \r\n')
        
        print_success "Database contains $table_count tables"
    else
        print_error "Database verification failed"
        exit 1
    fi
}

# Function to display connection information
display_connection_info() {
    echo ""
    echo "=================================="
    echo "🎉 SETUP COMPLETED SUCCESSFULLY! 🎉"
    echo "=================================="
    echo ""
    echo "Connection Information:"
    echo "======================"
    echo "Server: localhost,$SQL_PORT"
    echo "Database: $DATABASE_NAME"
    echo "Username: sa"
    echo "Password: $SA_PASSWORD"
    echo "Container: $CONTAINER_NAME"
    echo ""
    echo "You can now connect to your AdventureWorksLT2022 database!"
    echo ""
    echo "To stop the container: docker stop $CONTAINER_NAME"
    echo "To start the container: docker start $CONTAINER_NAME"
    echo "To remove the container: docker stop $CONTAINER_NAME && docker rm $CONTAINER_NAME"
    echo ""
}

# Main execution
main() {
    echo "======================================"
    echo "AdventureWorksLT2022 Setup Script"
    echo "======================================"
    echo ""
    
    check_docker
    check_backup_file
    cleanup_existing_container
    create_container
    wait_for_sql_server
    copy_backup_file
    get_logical_names
    restore_database
    verify_database
    display_connection_info
}

# Show usage if help is requested
if [[ "$1" == "-h" || "$1" == "--help" ]]; then
    echo "AdventureWorksLT2022 SQL Server Setup Script"
    echo ""
    echo "Usage: $0"
    echo ""
    echo "This script will:"
    echo "1. Create a SQL Server 2022 Docker container"
    echo "2. Copy the AdventureWorksLT2022.bak file to the container"
    echo "3. Restore the database"
    echo "4. Verify the setup"
    echo ""
    echo "Prerequisites:"
    echo "- Docker must be installed and running"
    echo "- AdventureWorksLT2022.bak file must be in the current directory"
    echo ""
    echo "Configuration (you can modify these in the script):"
    echo "- Container name: $CONTAINER_NAME"
    echo "- SA password: $SA_PASSWORD"
    echo "- SQL port: $SQL_PORT"
    echo "- Database name: $DATABASE_NAME"
    exit 0
fi

# Run main function
main "$@"
