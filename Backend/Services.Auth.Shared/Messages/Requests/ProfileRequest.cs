﻿namespace Services.Auth.Shared.Messages.Requests;

[Serializable]
public class ProfileRequest
{
    public required string ConnectionId;
    
    public required string AccessToken;
}