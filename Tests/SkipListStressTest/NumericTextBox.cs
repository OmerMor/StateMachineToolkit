using System;
using System.Windows.Forms;

namespace SkipListStressTest
{
	/// <summary>
	/// A text box for only excepting integers.
	/// </summary>
	public class NumericTextBox : TextBox
	{
		/// <summary>
		/// Event handler for the key press event.
		/// </summary>
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			// Filter out non numbers.
			if (!Char.IsNumber(e.KeyChar))
			{
				e.Handled = true;
			}
		}
	}
}