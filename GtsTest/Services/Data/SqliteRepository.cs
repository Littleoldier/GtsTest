// Services/SqliteRepository.cs
using System;
using System.Collections.Generic;
using System.Data;
using GtsTest.Core;
using GtsTest.Models;
using Microsoft.Data.Sqlite;

namespace GtsTest.Services.Data
{
    public class SqliteRepository : IDataRepository, IDisposable
    {
        private readonly string _dbPath;
        private readonly string _connStr;
        private bool _disposed = false;

        public SqliteRepository(string? dbPath = null)
        {
            _dbPath = dbPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gts.db");
            _connStr = new SqliteConnectionStringBuilder { DataSource = _dbPath }.ToString();
            EnsureDatabase();
        }

        private void EnsureDatabase()
        {
            using var conn = new SqliteConnection(_connStr);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                Salt TEXT,
                FullName TEXT,
                Role TEXT,
                IsActive INTEGER,
                CreatedTime TEXT,
                FailedAttempts INTEGER DEFAULT 0,
                LockoutUntil TEXT
            );
            ";
            cmd.ExecuteNonQuery();
        }

        public User? GetUserByUsername(string username)
        {
            using var conn = new SqliteConnection(_connStr);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Username, PasswordHash, Salt, FullName, Role, IsActive, CreatedTime, FailedAttempts, LockoutUntil FROM Users WHERE Username = @u LIMIT 1;";
            cmd.Parameters.AddWithValue("@u", username);
            using var rdr = cmd.ExecuteReader();
            if (!rdr.Read()) return null;
            return new User
            {
                Id = rdr.GetInt64(0),
                Username = rdr.GetString(1),
                PasswordHash = rdr.IsDBNull(2) ? "" : rdr.GetString(2),
                Salt = rdr.IsDBNull(3) ? null : rdr.GetString(3),
                FullName = rdr.IsDBNull(4) ? "" : rdr.GetString(4),
                Role = rdr.IsDBNull(5) ? "Operator" : rdr.GetString(5),
                IsActive = rdr.IsDBNull(6) ? 1 : rdr.GetInt32(6),
                CreatedTime = rdr.IsDBNull(7) ? DateTime.UtcNow.ToString("o") : rdr.GetString(7),
                FailedAttempts = rdr.IsDBNull(8) ? 0 : rdr.GetInt32(8),
                LockoutUntil = rdr.IsDBNull(9) ? null : rdr.GetString(9)
            };
        }

        public bool AddUser(User user)
        {
            try
            {
                using var conn = new SqliteConnection(_connStr);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
INSERT INTO Users (Username, PasswordHash, Salt, FullName, Role, IsActive, CreatedTime, FailedAttempts, LockoutUntil)
VALUES (@u, @ph, @s, @fn, @r, @ia, @ct, @fa, @lu);";
                cmd.Parameters.AddWithValue("@u", user.Username);
                cmd.Parameters.AddWithValue("@ph", user.PasswordHash);
                cmd.Parameters.AddWithValue("@s", (object?)user.Salt ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@fn", user.FullName);
                cmd.Parameters.AddWithValue("@r", user.Role);
                cmd.Parameters.AddWithValue("@ia", user.IsActive);
                cmd.Parameters.AddWithValue("@ct", user.CreatedTime);
                cmd.Parameters.AddWithValue("@fa", user.FailedAttempts);
                cmd.Parameters.AddWithValue("@lu", (object?)user.LockoutUntil ?? DBNull.Value);
                var n = cmd.ExecuteNonQuery();
                return n > 0;
            }
            catch (SqliteException ex)
            {
                AppLogger.Warn($"AddUser DB error: {ex.Message}", "SqliteRepository");
                return false;
            }
            catch (Exception ex)
            {
                AppLogger.Warn($"AddUser error: {ex.Message}", "SqliteRepository");
                return false;
            }
        }

        public bool UpdateUser(User user)
        {
            try
            {
                using var conn = new SqliteConnection(_connStr);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                UPDATE Users SET
                    PasswordHash = @ph,
                    Salt = @s,
                    FullName = @fn,
                    Role = @r,
                    IsActive = @ia,
                    FailedAttempts = @fa,
                    LockoutUntil = @lu
                WHERE Username = @u;";
                cmd.Parameters.AddWithValue("@ph", user.PasswordHash);
                cmd.Parameters.AddWithValue("@s", (object?)user.Salt ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@fn", user.FullName);
                cmd.Parameters.AddWithValue("@r", user.Role);
                cmd.Parameters.AddWithValue("@ia", user.IsActive);
                cmd.Parameters.AddWithValue("@fa", user.FailedAttempts);
                cmd.Parameters.AddWithValue("@lu", (object?)user.LockoutUntil ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@u", user.Username);
                var n = cmd.ExecuteNonQuery();
                return n > 0;
            }
            catch (Exception ex)
            {
                AppLogger.Warn($"UpdateUser error: {ex.Message}", "SqliteRepository");
                return false;
            }
        }

        public List<User> GetAllUsers()
        {
            var list = new List<User>();
            using var conn = new SqliteConnection(_connStr);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Username, PasswordHash, Salt, FullName, Role, IsActive, CreatedTime, FailedAttempts, LockoutUntil FROM Users;";
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                list.Add(new User
                {
                    Id = rdr.GetInt64(0),
                    Username = rdr.GetString(1),
                    PasswordHash = rdr.IsDBNull(2) ? "" : rdr.GetString(2),
                    Salt = rdr.IsDBNull(3) ? null : rdr.GetString(3),
                    FullName = rdr.IsDBNull(4) ? "" : rdr.GetString(4),
                    Role = rdr.IsDBNull(5) ? "Operator" : rdr.GetString(5),
                    IsActive = rdr.IsDBNull(6) ? 1 : rdr.GetInt32(6),
                    CreatedTime = rdr.IsDBNull(7) ? DateTime.UtcNow.ToString("o") : rdr.GetString(7),
                    FailedAttempts = rdr.IsDBNull(8) ? 0 : rdr.GetInt32(8),
                    LockoutUntil = rdr.IsDBNull(9) ? null : rdr.GetString(9)
                });
            }
            return list;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }
    }
}