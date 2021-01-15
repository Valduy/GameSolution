﻿namespace MatchmakerModels.Models.Interfaces
{
    public interface IMatchmakerModel
    {
        bool Enqueue(string userId);
        UserStatus GetStatus(string userId);
        uint? GetMatch(string userId);
    }
}
