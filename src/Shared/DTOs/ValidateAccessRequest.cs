namespace Shared.DTOs;

public class ValidateAccessRequest
{
    public int? UserId { get; set; }
    public int? CredentialId { get; set; }
    public int ControlPointId { get; set; }
}
