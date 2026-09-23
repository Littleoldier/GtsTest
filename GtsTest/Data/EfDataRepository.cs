using GtsTest.Data;
using GtsTest.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GtsTest.Services.Data
{
    public class EfDataRepository : IDataRepository, IDisposable
    {
        private readonly GtsDbContext _db;

        public EfDataRepository()
        {
            _db = DbContextFactory.Create();
        }

        public EfDataRepository(GtsDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        // ============ 用户管理 ============
        public User? GetUserByUsername(string username)
        {
            return _db.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.Username == username && u.IsDeleted == 0);
        }

        public bool AddUser(User user)
        {
            try
            {
                _db.Users.Add(user);
                return _db.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                Core.AppLogger.Warn($"AddUser 失败: {ex.Message}", "EfRepo");
                return false;
            }
        }

        public bool UpdateUser(User user)
        {
            try
            {
                var existing = _db.Users.Find(user.Id);
                if (existing == null) return false;
                _db.Entry(existing).CurrentValues.SetValues(user);
                return _db.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                Core.AppLogger.Warn($"UpdateUser 失败: {ex.Message}", "EfRepo");
                return false;
            }
        }

        public List<User> GetAllUsers(bool includeDeleted = false)
        {
            var q = _db.Users.AsNoTracking();
            if (!includeDeleted)
                q = q.Where(u => u.IsDeleted == 0);
            return q.OrderBy(u => u.Username).ToList();
        }

        // ============ 逻辑删除 ============
        public bool SoftDeleteUser(long userId, string deletedBy)
        {
            try
            {
                var user = _db.Users.Find(userId);
                if (user == null) return false;
                user.IsDeleted = 1;
                user.DeletedTime = DateTime.UtcNow.ToString("o");
                user.DeletedBy = deletedBy ?? "系统";
                return _db.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                Core.AppLogger.Warn($"SoftDeleteUser 失败: {ex.Message}", "EfRepo");
                return false;
            }
        }

        public bool RestoreUser(long userId)
        {
            try
            {
                var user = _db.Users.Find(userId);
                if (user == null) return false;
                user.IsDeleted = 0;
                user.DeletedTime = null;
                user.DeletedBy = null;
                return _db.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                Core.AppLogger.Warn($"RestoreUser 失败: {ex.Message}", "EfRepo");
                return false;
            }
        }

        public void Dispose() => _db?.Dispose();
    }
}