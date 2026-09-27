using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class GameUpdateRequest
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public double Price { get; set; } = 0;
    }
}
