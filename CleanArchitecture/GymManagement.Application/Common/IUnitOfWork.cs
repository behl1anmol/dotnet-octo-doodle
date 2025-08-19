using System;

namespace GymManagement.Application.Common;

public interface IUnitOfWork
{
    Task CommitChangesAsync();
}
