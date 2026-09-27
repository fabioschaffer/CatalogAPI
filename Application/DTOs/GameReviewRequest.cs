namespace Application.DTOs
{
    public class GameReviewRequest
    {
        public int GameId { get; set; }
        public int Nota { get; set; }
        public string Comentario { get; set; } = string.Empty;
    }
}
