// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Models;

public sealed class Token : BaseEntity<Token, TokenId>, IAuditableEntity, IAggregateRoot
{
   /// <summary>
    /// Gets or sets the application associated with the current token.
    /// </summary>
    public Application? Application { get; set; }

    /// <summary>
    /// Gets or sets the authorization associated with the current token.
    /// </summary>
    public Authorization? Authorization { get; set; }

    /// <summary>
    /// Gets or sets the concurrency token.
    /// </summary>
    public string? ConcurrencyToken { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the UTC creation date of the current token.
    /// </summary>
    public DateTime? CreationDate { get; set; }

    /// <summary>
    /// Gets or sets the UTC expiration date of the current token.
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier associated with the current token.
    /// </summary>


    /// <summary>
    /// Gets or sets the payload of the current token, if applicable.
    /// Note: this property is only used for reference tokens
    /// and may be encrypted for security reasons.
    /// </summary>
    public string? Payload { get; set; }

    /// <summary>
    /// Gets or sets the additional properties serialized as a JSON object,
    /// or <see langword="null"/> if no bag was associated with the current token.
    /// </summary>
    [StringSyntax(StringSyntaxAttribute.Json)]
    public string? Properties { get; set; }

    /// <summary>
    /// Gets or sets the UTC redemption date of the current token.
    /// </summary>
    public DateTime? RedemptionDate { get; set; }

    /// <summary>
    /// Gets or sets the reference identifier associated
    /// with the current token, if applicable.
    /// Note: this property is only used for reference tokens
    /// and may be hashed or encrypted for security reasons.
    /// </summary>
    public string? ReferenceId { get; set; }

    /// <summary>
    /// Gets or sets the status of the current token.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets the subject associated with the current token.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the type of the current token.
    /// </summary>
    public string? Type { get; set; }

    private Token() { }

    public Token(TokenId id, Application? application, Authorization? authorization, string? concurrencyToken, DateTime? creationDate, DateTime? expirationDate, string? payload, string? properties, DateTime? redemptionDate, string? referenceId, string? status, string? subject, string? type)
        : base()
    {
        Application = application;
        Authorization = authorization;
        ConcurrencyToken = concurrencyToken;
        CreationDate = creationDate;
        ExpirationDate = expirationDate;
        Payload = payload;
        Properties = properties;
        RedemptionDate = redemptionDate;
        ReferenceId = referenceId;
        Status = status;
        Subject = subject;
        Type = type;

        RaiseDomainEvent(new TokenCreatedEvent(Id));
    }

    public void UpdateStatus(string newStatus)
    {
        if (Status == newStatus) return; // Avoid unnecessary updates

        Status = newStatus;
        RaiseDomainEvent(new TokenUpdatedEvent(Id));
    }

     public void UpdateToken(string? newStatus, DateTime? newExpirationDate)
     {
         var updated = false;

         if (Status != newStatus)
         {
             Status = newStatus;
             updated = true;
         }

         if (ExpirationDate != newExpirationDate)
         {
             ExpirationDate = newExpirationDate;
             updated = true;
         }

         if (updated)
         {
             RaiseDomainEvent(new TokenUpdatedEvent(Id));
         }
     }
   
}

