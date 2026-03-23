using StackExchange.Redis;

public class RedisServices
{
    private readonly IDatabase _db;

    public RedisServices(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task AddNotification(string userId, string message)
    {
        await _db.ListRightPushAsync($"notif:{userId}", message);
        await _db.StringIncrementAsync($"notif_count:{userId}");
    }

    public async Task<long> GetNotificationCount(string userId)
    {
        var value = await _db.StringGetAsync($"notif_count:{userId}");

        if (!value.HasValue)
            return 0;

        return (long)value; // ✅ FIXED casting
    }

    public async Task<List<string>> GetNotifications(string userId)
    {
        var values = await _db.ListRangeAsync($"notif:{userId}");
        return values.Select(v => v.ToString()).ToList();
    }

    public async Task ClearNotification(string userId)
    {
        await _db.StringSetAsync($"notif_count:{userId}", 0);
    }

    public async Task ClearAllNotifications(string userId)
    {
        await _db.KeyDeleteAsync($"notif:{userId}");
        await _db.KeyDeleteAsync($"notif_count:{userId}");
    }

    public async Task RemoveNotification(string userId, string message)
    {
        long removed = await _db.ListRemoveAsync($"notif:{userId}", message, 1);

        if (removed > 0)
        {
            var currentCount = await GetNotificationCount(userId);

            if (currentCount <= 1)
                await _db.StringSetAsync($"notif_count:{userId}", 0);
            else
                await _db.StringDecrementAsync($"notif_count:{userId}");
        }



    }

    private string GetOtpKey(string email)
    {
        return $"SolvePoint:PasswordReset:{email?.Trim().ToLower()}";
    }

    // ── OTP METHODS ─────────────────────────────

    // Save OTP (15 min expiry)
    public async Task SaveOtpAsync(string email, string otp, string userType)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp))
            return;

        var key = GetOtpKey(email);
        var value = $"{otp}|{userType}";

        await _db.StringSetAsync(key, value, TimeSpan.FromMinutes(15));
    }

    // Verify OTP
    public async Task<string?> VerifyOtpAsync(string email, string otp)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp))
            return null;

        var key = GetOtpKey(email);
        var stored = await _db.StringGetAsync(key);

        if (stored.IsNullOrEmpty)
            return null;

        var parts = stored.ToString().Split('|');
        if (parts.Length < 2)
            return null;

        var storedOtp = parts[0];
        var userType = parts[1];

        if (!storedOtp.Equals(otp.Trim()))
            return null;

        // ✅ Delete OTP after successful verification
        await _db.KeyDeleteAsync(key);

        return userType;
    }

    // Delete OTP manually
    public async Task DeleteOtpAsync(string email)
    {
        var key = GetOtpKey(email);
        await _db.KeyDeleteAsync(key);
    }

    // Check if OTP exists
    public async Task<bool> OtpExistsAsync(string email)
    {
        var key = GetOtpKey(email);
        return await _db.KeyExistsAsync(key);
    }

    // ── GENERIC METHODS ─────────────────────────

    public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        if (expiry.HasValue)
            await _db.StringSetAsync(key, value, expiry.Value);
        else
            await _db.StringSetAsync(key, value);
    }

    public async Task<string?> GetAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;

        var val = await _db.StringGetAsync(key);
        return val.IsNullOrEmpty ? null : val.ToString();
    }

    public async Task DeleteAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        await _db.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;

        return await _db.KeyExistsAsync(key);
    }
}