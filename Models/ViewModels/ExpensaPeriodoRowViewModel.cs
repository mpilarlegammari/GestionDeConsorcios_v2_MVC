using System;

namespace GestionDeConsorcios_v2_MVC.Models.ViewModels
{
    public class ExpensaPeriodoRowViewModel
    {
        public int ConsorcioId { get; set; }
        public string ConsorcioNombre { get; set; }
        public string Periodo { get; set; }
        public decimal MontoTotal { get; set; }
    }
}
