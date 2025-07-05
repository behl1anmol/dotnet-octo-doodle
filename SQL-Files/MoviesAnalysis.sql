select m.*
  , string_agg(distinct g.name, ', ') as genres
  , round(avg(r.rating),1) as rating
  , myr.rating as userrating
  from movies m
  left join genres g on m.id = g.movieId
  left join ratings r on m.id = r.movieid
  left join ratings myr on m.id = myr.movieid 
  where (null is null or m.title like ('%' || null || '%'))
  and (null is null or m.yearofrelease = null)
  group by id, userrating
  limit 1 offset 2
