using System.ComponentModel.DataAnnotations;

namespace StockTwse.ViewModels {
	public class UserInputViewModel {
		[Required]
		public string UserInput { get; set; } = null!;
	}
}
