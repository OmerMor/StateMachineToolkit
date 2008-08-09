using System;

namespace SkipListStressTest
{
	/// <summary>
	/// A class for shuffling integers.
	/// </summary>
	public class Shuffler
	{
		private readonly Random random = new Random();

		/// <summary>
		/// Shuffles an array of integers.
		/// </summary>
		/// <param name="array">
		/// The array to be shuffled.
		/// </param>
		public void Shuffle(int[] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				int j = random.Next(i, array.Length);
				int temp = array[i];
				array[i] = array[j];
				array[j] = temp;
			}
		}
	}
}