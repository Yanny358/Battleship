using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class Game
    {
        public int GameId { get; set; }

        public int GameOptionId { get; set; }
        public GameOption? GameOption { get; set; }
        
        public string BoardData { get; set; } = null!;


        [MaxLength(128)]
        public string Description { get; set; } = DateTime.Now.ToLongDateString();

    }
}