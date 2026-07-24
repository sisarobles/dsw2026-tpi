namespace Dsw2026Tpi.Domain.Entities;

public class Patient : EntityBase
{
    public string UserId { get; init; }
    public long Dni { get; init; }
    public string? FullName { get; private set; }
    public bool IsActive { get; private set; }

    #region Constructor for EF
#pragma warning disable CS8618
    private Patient()
    {
    }
#pragma warning restore CS8618
    #endregion

    public Patient(string userId, long dni, string? fullName = null, Guid? id = null) : base(id)
    {
        UserId = userId;
        Dni = dni;
        FullName = fullName;
        IsActive = true;
    }

    public void UpdateFullName(string fullName)
    {
        FullName = fullName;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
