using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            _context = context;
        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Like> GetLike(int likerId, int likeeId)
        {
            return await _context.Likes.FirstOrDefaultAsync(l => 
               l.LikerId == likerId && l.LikeeId == likeeId
            );
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(u => u.UserId == userId)
                        .FirstOrDefaultAsync(p => p.IsMain == true);
        }

        public async Task<Photo> GetPhoto(int id) =>
            await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);

        public async Task<User> GetUser(int id) =>
            await _context.Users
                    .Include(p => p.Photos)
                    .FirstOrDefaultAsync(u => u.Id == id);

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users =  _context.Users.Include(p => p.Photos)
                .OrderByDescending(u => u.LastActive)
                .AsQueryable();

            users = users.Where(u => u.Id != userParams.UserId);
            users = users.Where( u => u.Gender == userParams.Gender);

            if (userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikers.Contains(u.Id));
            }

            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikees.Contains(u.Id));
            }


            if (userParams.maxAge != 99 || userParams.maxAge != 18)
            {
                var minDob = DateTime.Today.AddYears(-userParams.maxAge - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where( u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }
            
            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch(userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _context.Users
                                .Include(u => u.Likers)
                                .Include(u => u.Likees)
                                .FirstOrDefaultAsync(u => u.Id == id);

            if (likers)
            {
                return user.Likers.Where(l => l.LikeeId == id).Select(l => l.LikerId);
            } else {
                return user.Likees.Where(l => l.LikerId == id).Select(l => l.LikeeId);
            }
        }
        public async Task<bool> SaveAll() =>
            await _context.SaveChangesAsync() > 0;

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages
                .Include(m => m.Sender).ThenInclude(u => u.Photos)
                .Include(m => m.Recipient).ThenInclude(u => u.Photos)
                .AsQueryable();

                switch(messageParams.MessageContainer)
                {
                    case "Inbox":
                        messages = messages.Where(m => m.RecipientId == messageParams.UserId && !m.RecipientDeleted);
                        break;
                    case "Outbox":
                        messages = messages.Where(m => m.SenderId == messageParams.UserId && !m.SenderDeleted);
                        break;
                    default:
                        messages = messages.Where(m => m.RecipientId == messageParams.UserId && !m.RecipientDeleted && m.IsRead == false);
                    break;
                }

                messages = messages.OrderByDescending(m => m.MessageSent);
                return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
            => await _context.Messages
                .Include(m => m.Sender).ThenInclude(u => u.Photos)
                .Include(m => m.Recipient).ThenInclude(u => u.Photos)
                .Where(m => m.RecipientId == userId && m.SenderId == recipientId && !m.RecipientDeleted
                    || m.RecipientId == recipientId && m.SenderId == userId && !m.SenderDeleted)
                .OrderByDescending(m => m.MessageSent)
                .ToListAsync();
    }
}