namespace Infrastructure.Services.Caching;

/// <summary>
/// Constants for cache keys and patterns.
/// </summary>
public static class CacheKeys
{
    // TTL configurations (in minutes)
    public static class Ttl
    {
        public const int ActiveBenefits = 30; // 30 minutes for active benefits
        public const int AccessRules = 60; // 60 minutes for access rules
        public const int BenefitTypes = 120; // 2 hours for benefit types (rarely change)
        public const int UserProfile = 15; // 15 minutes for user profiles
    }

    // Benefit cache keys
    public static class Benefits
    {
        public const string Prefix = "benefits";
        
        public static string Active(int tenantId) => $"{Prefix}:tenant:{tenantId}:active";
        public static string ById(int tenantId, int benefitId) => $"{Prefix}:tenant:{tenantId}:id:{benefitId}";
        public static string ByType(int tenantId, int benefitTypeId) => $"{Prefix}:tenant:{tenantId}:type:{benefitTypeId}";
        public static string All(int tenantId) => $"{Prefix}:tenant:{tenantId}:all";
        
        public static string Pattern(int tenantId) => $"{Prefix}:tenant:{tenantId}:*";
        public static string PatternAll() => $"{Prefix}:*";
    }

    // Access Rule cache keys
    public static class AccessRules
    {
        public const string Prefix = "accessrules";
        
        public static string ById(int tenantId, int ruleId) => $"{Prefix}:tenant:{tenantId}:id:{ruleId}";
        public static string ByControlPoint(int tenantId, int controlPointId) => $"{Prefix}:tenant:{tenantId}:controlpoint:{controlPointId}";
        public static string All(int tenantId) => $"{Prefix}:tenant:{tenantId}:all";
        public static string Active(int tenantId) => $"{Prefix}:tenant:{tenantId}:active";
        
        public static string Pattern(int tenantId) => $"{Prefix}:tenant:{tenantId}:*";
        public static string PatternAll() => $"{Prefix}:*";
    }

    // Benefit Type cache keys
    public static class BenefitTypes
    {
        public const string Prefix = "benefittypes";
        
        public static string ById(int tenantId, int typeId) => $"{Prefix}:tenant:{tenantId}:id:{typeId}";
        public static string All(int tenantId) => $"{Prefix}:tenant:{tenantId}:all";
        
        public static string Pattern(int tenantId) => $"{Prefix}:tenant:{tenantId}:*";
        public static string PatternAll() => $"{Prefix}:*";
    }

    // User cache keys
    public static class Users
    {
        public const string Prefix = "users";
        
        public static string ById(int tenantId, int userId) => $"{Prefix}:tenant:{tenantId}:id:{userId}";
        public static string ByEmail(int tenantId, string email) => $"{Prefix}:tenant:{tenantId}:email:{email.ToLowerInvariant()}";
        
        public static string Pattern(int tenantId) => $"{Prefix}:tenant:{tenantId}:*";
        public static string PatternAll() => $"{Prefix}:*";
    }
}
