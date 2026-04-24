using Microsoft.AspNetCore.Mvc;

namespace CSharpPost.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private static readonly List<Post> _posts = new List<Post>();
        private static readonly List<Draft> _drafts = new List<Draft>();
        private static readonly List<ScheduledPost> _scheduledPosts = new List<ScheduledPost>();
        private static readonly List<Activity> _activities = new List<Activity>();
        private static readonly List<Notification> _notifications = new List<Notification>();
        private static readonly List<Bookmark> _bookmarks = new List<Bookmark>();
        private static readonly User _defaultUser = new User { Id = 1, Username = "CSharpPost" };
        private static int _nextId = 1;
        private static int _draftId = 1;
        private static int _scheduledId = 1;
        private static int _activityId = 1;
        private static int _notificationId = 1;
        private static int _bookmarkId = 1;

        [HttpPost]
        public IActionResult CreatePost([FromForm] CreatePostRequest request)
        {
            if (string.IsNullOrEmpty(request.Text))
                return BadRequest("Post text is required.");

            if (request.Text.Length > 140)
                return BadRequest("Post text exceeds 140 characters.");

            var hashtags = ExtractHashtags(request.Text);
            var mentions = ExtractMentions(request.Text);

            var post = new Post
            {
                Id = _nextId++,
                Text = request.Text,
                CreatedAt = DateTime.UtcNow,
                User = _defaultUser,
                Likes = 0,
                Hashtags = hashtags,
                Mentions = mentions,
                IsEdited = false
            };

            _posts.Insert(0, post);

            var activity = new Activity
            {
                Id = _activityId++,
                Type = "post_created",
                Description = $"New post created: {post.Text.Substring(0, Math.Min(50, post.Text.Length))}...",
                Timestamp = DateTime.UtcNow,
                PostId = post.Id,
                UserId = post.User.Id
            };
            _activities.Insert(0, activity);

            var notification = new Notification
            {
                Id = _notificationId++,
                Type = "new_post",
                Message = "New post created successfully!",
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };
            _notifications.Insert(0, notification);

            var result = new
            {
                id = post.Id,
                text = post.Text,
                createdAt = post.CreatedAt,
                user = new { username = post.User.Username },
                likes = post.Likes,
                hashtags = post.Hashtags,
                mentions = post.Mentions,
                isEdited = post.IsEdited,
                characterCount = post.Text.Length
            };

            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, result);
        }

        [HttpGet]
        public IActionResult GetTimeline(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string search = "",
            [FromQuery] string sortBy = "latest")
        {
            var query = _posts.AsEnumerable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(post => 
                    post.Text.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    post.Hashtags.Any(tag => tag.Contains(search, StringComparison.OrdinalIgnoreCase)));
            }

            query = sortBy switch
            {
                "popular" => query.OrderByDescending(post => post.Likes),
                "oldest" => query.OrderBy(post => post.CreatedAt),
                _ => query.OrderByDescending(post => post.CreatedAt)
            };

            var totalPosts = query.Count();
            var posts = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(post => new
                {
                    id = post.Id,
                    text = post.Text,
                    createdAt = post.CreatedAt,
                    user = new { username = post.User.Username },
                    likes = post.Likes,
                    hashtags = post.Hashtags,
                    mentions = post.Mentions,
                    isEdited = post.IsEdited,
                    characterCount = post.Text.Length
                })
                .ToList();

            return Ok(new
            {
                totalPages = (int)Math.Ceiling(totalPosts / (double)pageSize),
                currentPage = page,
                posts = posts,
                totalPosts = totalPosts
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetPost(int id)
        {
            var post = _posts.FirstOrDefault(p => p.Id == id);
            if (post == null)
                return NotFound();

            var result = new
            {
                id = post.Id,
                text = post.Text,
                createdAt = post.CreatedAt,
                user = new { username = post.User.Username },
                likes = post.Likes,
                hashtags = post.Hashtags,
                mentions = post.Mentions,
                isEdited = post.IsEdited,
                characterCount = post.Text.Length
            };

            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult EditPost(int id, [FromBody] EditPostRequest request)
        {
            var post = _posts.FirstOrDefault(p => p.Id == id);
            if (post == null)
                return NotFound();

            if (string.IsNullOrEmpty(request.Text))
                return BadRequest("Post text is required.");

            if (request.Text.Length > 140)
                return BadRequest("Post text exceeds 140 characters.");

            post.Text = request.Text;
            post.UpdatedAt = DateTime.UtcNow;
            post.IsEdited = true;
            post.Hashtags = ExtractHashtags(request.Text);
            post.Mentions = ExtractMentions(request.Text);

            var result = new
            {
                id = post.Id,
                text = post.Text,
                createdAt = post.CreatedAt,
                updatedAt = post.UpdatedAt,
                user = new { username = post.User.Username },
                likes = post.Likes,
                hashtags = post.Hashtags,
                mentions = post.Mentions,
                isEdited = post.IsEdited,
                characterCount = post.Text.Length
            };

            return Ok(result);
        }

        [HttpPost("{id}/like")]
        public IActionResult LikePost(int id)
        {
            var post = _posts.FirstOrDefault(p => p.Id == id);
            if (post == null)
                return NotFound();

            post.Likes++;

            var activity = new Activity
            {
                Id = _activityId++,
                Type = "post_liked",
                Description = $"Post liked: {post.Text.Substring(0, Math.Min(30, post.Text.Length))}...",
                Timestamp = DateTime.UtcNow,
                PostId = post.Id,
                UserId = post.User.Id
            };
            _activities.Insert(0, activity);

            return Ok(new { likes = post.Likes });
        }

        [HttpPost("{id}/unlike")]
        public IActionResult UnlikePost(int id)
        {
            var post = _posts.FirstOrDefault(p => p.Id == id);
            if (post == null)
                return NotFound();

            if (post.Likes > 0)
                post.Likes--;

            return Ok(new { likes = post.Likes });
        }

        [HttpGet("hashtags")]
        public IActionResult GetTrendingHashtags()
        {
            var hashtagCounts = _posts
                .SelectMany(post => post.Hashtags)
                .GroupBy(tag => tag.ToLower())
                .Select(g => new { hashtag = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .Take(10)
                .ToList();

            return Ok(hashtagCounts);
        }

        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            var stats = new
            {
                totalPosts = _posts.Count,
                totalLikes = _posts.Sum(p => p.Likes),
                averageLikes = _posts.Count > 0 ? _posts.Average(p => p.Likes) : 0,
                mostPopularPost = _posts.OrderByDescending(p => p.Likes).FirstOrDefault(),
                postsToday = _posts.Count(p => p.CreatedAt.Date == DateTime.Today)
            };

            return Ok(stats);
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePost(int id)
        {
            var post = _posts.FirstOrDefault(p => p.Id == id);
            if (post == null)
                return NotFound();

            _posts.Remove(post);

            var activity = new Activity
            {
                Id = _activityId++,
                Type = "post_deleted",
                Description = $"Post deleted: {post.Text.Substring(0, Math.Min(30, post.Text.Length))}...",
                Timestamp = DateTime.UtcNow,
                PostId = id,
                UserId = post.User.Id
            };
            _activities.Insert(0, activity);

            return Ok(new { message = "Post deleted" });
        }

        [HttpGet("activities")]
        public IActionResult GetActivities([FromQuery] int limit = 20)
        {
            var activities = _activities
                .Take(limit)
                .Select(a => new
                {
                    id = a.Id,
                    type = a.Type,
                    description = a.Description,
                    timestamp = a.Timestamp,
                    postId = a.PostId
                })
                .ToList();

            return Ok(activities);
        }

        [HttpGet("notifications")]
        public IActionResult GetNotifications([FromQuery] bool unreadOnly = false)
        {
            var notifications = _notifications.AsEnumerable();

            if (unreadOnly)
            {
                notifications = notifications.Where(n => !n.IsRead);
            }

            var result = notifications
                .Take(10)
                .Select(n => new
                {
                    id = n.Id,
                    type = n.Type,
                    message = n.Message,
                    timestamp = n.Timestamp,
                    isRead = n.IsRead
                })
                .ToList();

            return Ok(new { notifications = result, unreadCount = _notifications.Count(n => !n.IsRead) });
        }

        [HttpPost("notifications/{id}/read")]
        public IActionResult MarkNotificationRead(int id)
        {
            var notification = _notifications.FirstOrDefault(n => n.Id == id);
            if (notification == null)
                return NotFound();

            notification.IsRead = true;
            return Ok(new { message = "Notification marked as read" });
        }

        [HttpPost("notifications/read-all")]
        public IActionResult MarkAllNotificationsRead()
        {
            foreach (var notification in _notifications)
            {
                notification.IsRead = true;
            }
            return Ok(new { message = "All notifications marked as read" });
        }

        [HttpPost("drafts")]
        public IActionResult SaveDraft([FromBody] CreatePostRequest request)
        {
            if (string.IsNullOrEmpty(request.Text))
                return BadRequest("Draft text is required.");

            var draft = new Draft
            {
                Id = _draftId++,
                Text = request.Text,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                User = _defaultUser
            };

            _drafts.Insert(0, draft);

            return Ok(new { 
                id = draft.Id,
                text = draft.Text,
                createdAt = draft.CreatedAt,
                updatedAt = draft.UpdatedAt
            });
        }

        [HttpGet("drafts")]
        public IActionResult GetDrafts()
        {
            var drafts = _drafts
                .Select(d => new
                {
                    id = d.Id,
                    text = d.Text,
                    createdAt = d.CreatedAt,
                    updatedAt = d.UpdatedAt,
                    characterCount = d.Text.Length
                })
                .ToList();

            return Ok(drafts);
        }

        [HttpDelete("drafts/{id}")]
        public IActionResult DeleteDraft(int id)
        {
            var draft = _drafts.FirstOrDefault(d => d.Id == id);
            if (draft == null)
                return NotFound();

            _drafts.Remove(draft);
            return Ok(new { message = "Draft deleted" });
        }

        [HttpPost("schedule")]
        public IActionResult SchedulePost([FromBody] SchedulePostRequest request)
        {
            if (string.IsNullOrEmpty(request.Text))
                return BadRequest("Post text is required.");

            if (request.Text.Length > 140)
                return BadRequest("Post text exceeds 140 characters.");

            if (request.ScheduledAt <= DateTime.UtcNow)
                return BadRequest("Scheduled time must be in the future.");

            var scheduledPost = new ScheduledPost
            {
                Id = _scheduledId++,
                Text = request.Text,
                ScheduledAt = request.ScheduledAt,
                CreatedAt = DateTime.UtcNow,
                User = _defaultUser,
                Hashtags = ExtractHashtags(request.Text),
                Mentions = ExtractMentions(request.Text)
            };

            _scheduledPosts.Add(scheduledPost);

            return Ok(new { 
                id = scheduledPost.Id,
                text = scheduledPost.Text,
                scheduledAt = scheduledPost.ScheduledAt,
                createdAt = scheduledPost.CreatedAt
            });
        }

        [HttpGet("scheduled")]
        public IActionResult GetScheduledPosts()
        {
            var scheduled = _scheduledPosts
                .Where(sp => sp.ScheduledAt > DateTime.UtcNow)
                .OrderBy(sp => sp.ScheduledAt)
                .Select(sp => new
                {
                    id = sp.Id,
                    text = sp.Text,
                    scheduledAt = sp.ScheduledAt,
                    createdAt = sp.CreatedAt,
                    hashtags = sp.Hashtags,
                    mentions = sp.Mentions,
                    characterCount = sp.Text.Length
                })
                .ToList();

            return Ok(scheduled);
        }

        [HttpDelete("scheduled/{id}")]
        public IActionResult CancelScheduledPost(int id)
        {
            var scheduledPost = _scheduledPosts.FirstOrDefault(sp => sp.Id == id);
            if (scheduledPost == null)
                return NotFound();

            _scheduledPosts.Remove(scheduledPost);
            return Ok(new { message = "Scheduled post cancelled" });
        }

        [HttpPost("publish-draft/{id}")]
        public IActionResult PublishDraft(int id)
        {
            var draft = _drafts.FirstOrDefault(d => d.Id == id);
            if (draft == null)
                return NotFound();

            if (draft.Text.Length > 140)
                return BadRequest("Draft text exceeds 140 characters.");

            var post = new Post
            {
                Id = _nextId++,
                Text = draft.Text,
                CreatedAt = DateTime.UtcNow,
                User = _defaultUser,
                Likes = 0,
                Hashtags = ExtractHashtags(draft.Text),
                Mentions = ExtractMentions(draft.Text),
                IsEdited = false
            };

            _posts.Insert(0, post);
            _drafts.Remove(draft);

            return Ok(new { 
                id = post.Id,
                text = post.Text,
                createdAt = post.CreatedAt,
                user = new { username = post.User.Username },
                likes = post.Likes,
                hashtags = post.Hashtags,
                mentions = post.Mentions,
                isEdited = post.IsEdited
            });
        }

        [HttpGet("analytics")]
        public IActionResult GetAnalytics()
        {
            var totalPosts = _posts.Count;
            var totalLikes = _posts.Sum(p => p.Likes);
            var totalHashtags = _posts.SelectMany(p => p.Hashtags).Count();
            var totalMentions = _posts.SelectMany(p => p.Mentions).Count();
            
            var postsByHour = _posts
                .GroupBy(p => p.CreatedAt.Hour)
                .Select(g => new { hour = g.Key, count = g.Count() })
                .OrderBy(x => x.hour)
                .ToList();

            var postsByDay = _posts
                .GroupBy(p => p.CreatedAt.DayOfWeek)
                .Select(g => new { day = g.Key.ToString(), count = g.Count() })
                .ToList();

            var topHashtags = _posts
                .SelectMany(p => p.Hashtags)
                .GroupBy(tag => tag.ToLower())
                .Select(g => new { hashtag = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .Take(10)
                .ToList();

            var topMentions = _posts
                .SelectMany(p => p.Mentions)
                .GroupBy(mention => mention.ToLower())
                .Select(g => new { mention = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .Take(10)
                .ToList();

            var engagementRate = totalPosts > 0 ? (double)totalLikes / totalPosts : 0;
            var avgPostLength = totalPosts > 0 ? _posts.Average(p => p.Text.Length) : 0;
            var postsWithHashtags = _posts.Count(p => p.Hashtags.Any());
            var postsWithMentions = _posts.Count(p => p.Mentions.Any());

            var analytics = new
            {
                overview = new
                {
                    totalPosts,
                    totalLikes,
                    totalHashtags,
                    totalMentions,
                    engagementRate = Math.Round(engagementRate, 2),
                    avgPostLength = Math.Round(avgPostLength, 1),
                    postsWithHashtags,
                    postsWithMentions,
                    hashtagUsageRate = totalPosts > 0 ? Math.Round((double)postsWithHashtags / totalPosts * 100, 1) : 0,
                    mentionUsageRate = totalPosts > 0 ? Math.Round((double)postsWithMentions / totalPosts * 100, 1) : 0
                },
                timeline = new
                {
                    postsByHour,
                    postsByDay
                },
                topContent = new
                {
                    topHashtags,
                    topMentions,
                    mostLikedPost = _posts.OrderByDescending(p => p.Likes).FirstOrDefault(),
                    mostEngagingPost = _posts.OrderByDescending(p => p.Likes + p.Hashtags.Count + p.Mentions.Count).FirstOrDefault()
                },
                trends = new
                {
                    todayActivity = _posts.Count(p => p.CreatedAt.Date == DateTime.Today),
                    weeklyActivity = _posts.Count(p => p.CreatedAt >= DateTime.Today.AddDays(-7)),
                    monthlyActivity = _posts.Count(p => p.CreatedAt >= DateTime.Today.AddDays(-30)),
                    growthRate = CalculateGrowthRate()
                }
            };

            return Ok(analytics);
        }

        private double CalculateGrowthRate()
        {
            var recentPosts = _posts.Count(p => p.CreatedAt >= DateTime.Today.AddDays(-7));
            var olderPosts = _posts.Count(p => p.CreatedAt >= DateTime.Today.AddDays(-14) && p.CreatedAt < DateTime.Today.AddDays(-7));
            
            if (olderPosts == 0) return recentPosts > 0 ? 100 : 0;
            return Math.Round((double)(recentPosts - olderPosts) / olderPosts * 100, 1);
        }

        [HttpPost("{id}/bookmark")]
        public IActionResult BookmarkPost(int id)
        {
            var post = _posts.FirstOrDefault(p => p.Id == id);
            if (post == null)
                return NotFound();

            var existingBookmark = _bookmarks.FirstOrDefault(b => b.PostId == id);
            if (existingBookmark != null)
                return BadRequest("Post already bookmarked");

            var bookmark = new Bookmark
            {
                Id = _bookmarkId++,
                PostId = id,
                PostText = post.Text,
                CreatedAt = DateTime.UtcNow,
                UserId = _defaultUser.Id
            };

            _bookmarks.Insert(0, bookmark);

            return Ok(new { 
                id = bookmark.Id,
                postId = bookmark.PostId,
                createdAt = bookmark.CreatedAt
            });
        }

        [HttpDelete("{id}/bookmark")]
        public IActionResult RemoveBookmark(int id)
        {
            var bookmark = _bookmarks.FirstOrDefault(b => b.PostId == id);
            if (bookmark == null)
                return NotFound();

            _bookmarks.Remove(bookmark);
            return Ok(new { message = "Bookmark removed" });
        }

        [HttpGet("bookmarks")]
        public IActionResult GetBookmarks()
        {
            var bookmarks = _bookmarks
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    id = b.Id,
                    postId = b.PostId,
                    postText = b.PostText,
                    createdAt = b.CreatedAt
                })
                .ToList();

            return Ok(bookmarks);
        }

        [HttpGet("{id}/is-bookmarked")]
        public IActionResult IsBookmarked(int id)
        {
            var isBookmarked = _bookmarks.Any(b => b.PostId == id);
            return Ok(new { isBookmarked });
        }

        private List<string> ExtractHashtags(string text)
        {
            return text.Split(' ')
                .Where(word => word.StartsWith('#'))
                .Select(word => word.Trim('#'))
                .Distinct()
                .ToList();
        }

        private List<string> ExtractMentions(string text)
        {
            return text.Split(' ')
                .Where(word => word.StartsWith('@'))
                .Select(word => word.Trim('@'))
                .Distinct()
                .ToList();
        }
    }

    public class CreatePostRequest
    {
        public string Text { get; set; }
    }

    public class UpdatePostRequest
    {
        public string Text { get; set; }
    }

    public class Activity
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
    }

    public class Notification
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
    }

    public class Draft
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public User User { get; set; }
    }

    public class ScheduledPost
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime ScheduledAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; }
        public List<string> Hashtags { get; set; } = new List<string>();
        public List<string> Mentions { get; set; } = new List<string>();
    }

    public class SchedulePostRequest
    {
        public string Text { get; set; }
        public DateTime ScheduledAt { get; set; }
    }

    public class Bookmark
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string PostText { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
    }

    public class Post
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public User User { get; set; }
        public int Likes { get; set; }
        public List<string> Hashtags { get; set; } = new List<string>();
        public List<string> Mentions { get; set; } = new List<string>();
        public bool IsEdited { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
    }
}
