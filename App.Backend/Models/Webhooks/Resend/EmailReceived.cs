// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace App.Backend.Domain.Values.Webhooks.Resend;

public class EmailReceived
{
    [JsonPropertyName("type")]
    public required string Type { get; set; } // "email.received"

    [JsonPropertyName("created_at")]
    public required string CreatedAt { get; set; }

    [JsonPropertyName("data")]
    public required EmailReceivedData Data { get; set; }
}

public class EmailReceivedData
{
    [JsonPropertyName("email_id")]
    public required Guid EmailId { get; set; }

    [JsonPropertyName("created_at")]
    public required string CreatedAt { get; set; }

    [JsonPropertyName("from")]
    public required string From { get; set; }

    [JsonPropertyName("to")]
    public required string[] To { get; set; }

    [JsonPropertyName("cc")]
    public required string[] Cc { get; set; }

    [JsonPropertyName("bcc")]
    public required string[] Bcc { get; set; }

    [JsonPropertyName("received_for")]
    public required string[] ReceivedFor { get; set; }

    [JsonPropertyName("message_id")]
    public required string MessageId { get; set; }

    [JsonPropertyName("subject")]
    public required string Subject { get; set; }

    [JsonPropertyName("template_id")]
    public required Guid TemplateId { get; set; }

    [JsonPropertyName("tags")]
    public required Dictionary<string, string> Tags { get; set; }

    [JsonPropertyName("attachments")]
    public required EmailAttachment[] Attachments { get; set; }
}

public class EmailAttachment
{
    [JsonPropertyName("id")]
    public required Guid Id { get; set; }

    [JsonPropertyName("filename")]
    public required string Filename { get; set; }

    [JsonPropertyName("content_type")]
    public required string ContentType { get; set; }

    [JsonPropertyName("content_disposition")]
    public required string ContentDisposition { get; set; }

    [JsonPropertyName("content_id")]
    public required Guid ContentId { get; set; }
}