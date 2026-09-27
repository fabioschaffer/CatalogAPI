using System;

namespace Domain.Entities
{
    public class GameReview
    {
        public string Id { get; set; } = string.Empty;
        public int GameId { get; set; }
        public int UserId { get; set; }
        public int Nota { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}
