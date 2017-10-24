using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace HomeExercises
{
	public class NumberValidatorTests
	{
        [TestCase(3, 2, true, "asd", ExpectedResult = false, TestName = "When non-number")]
        [TestCase(3, 2, true, "a.s", ExpectedResult = false, TestName = "When non-number with dot")]
        [TestCase(3, 2, true, "!1.2", ExpectedResult = false, TestName = "When number with unacceptable sign")]

        [TestCase(3, 0, true, "1", ExpectedResult = true, TestName = "When integer")]
        [TestCase(3, 0, true, "1.", ExpectedResult = false, TestName = "When integer with dot")]
        [TestCase(4, 3, true, ".123", ExpectedResult = false, TestName = "When there isn't integer part")]

        [TestCase(3, 2, true, "1.23", ExpectedResult = true, TestName = "When valid number")]
        [TestCase(3, 2, true, "1,23", ExpectedResult = true, TestName = "When valid number with comma")]
        [TestCase(3, 2, false, "-1.2", ExpectedResult = true, TestName = "When valid negative number")]
        [TestCase(3, 2, true, "+1.2", ExpectedResult = true, TestName = "When valid positive number with plus")]
        [TestCase(3, 2, true, "0.0", ExpectedResult = true, TestName = "When number is zero")]

        [TestCase(3, 2, true, "+1.23", ExpectedResult = false, TestName = "When positive number with plus is bigger than max length")]
        [TestCase(3, 2, false, "-1.23", ExpectedResult = false, TestName = "When negative number is bigger than max length")]
        [TestCase(3, 2, true, "00.00", ExpectedResult = false, TestName = "When number without sign is bigger than max length")]
        [TestCase(17, 4, true, "0.00000", ExpectedResult = false, TestName = "When scale is bigger than max scale length")]

        [TestCase(17, 2, true, "-0.00", ExpectedResult = false, TestName = "When negative number at positive mode")]
        [TestCase(17, 2, true, null, ExpectedResult = false, TestName = "When input is null")]
        [TestCase(17, 2, true, "", ExpectedResult = false, TestName = "When input is empty")]
        public bool ValidateNumber(int precision, int scale, bool onlyPositive, string value)
	    {
	        return new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value);
	    }

	    [TestCase(0, 2, true, typeof(ArgumentException), "precision must be a positive number",
	        TestName = "When precision is zero")]
        [TestCase(-1, 2, true, typeof(ArgumentException), "precision must be a positive number",
            TestName = "When precision is negative")]
        [TestCase(1, -1, false, typeof(ArgumentException), "scale must be a non-negative number less than precision",
            TestName = "When scale is negative")]
        [TestCase(1, 2, false, typeof(ArgumentException), "scale must be a non-negative number less than precision",
            TestName = "When scale is bigger than precision")]
	    [TestCase(2, 2, false, typeof(ArgumentException), "scale must be a non-negative number less than precision",
	        TestName = "When scale is equal precision")]
        public static void Throw(int precision, int scale, bool onlyPositive, Type expectedException, string expectedMessage)
	    {
	        Assert.That(() => new NumberValidator(precision, scale, onlyPositive), Throws.Exception.TypeOf(expectedException).With.Message.EqualTo(expectedMessage));
	    }
    }

	public class NumberValidator
	{
		private readonly Regex numberRegex;
		private readonly bool onlyPositive;
		private readonly int precision;
		private readonly int scale;

		public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
		{
			this.precision = precision;
			this.scale = scale;
			this.onlyPositive = onlyPositive;
			if (precision <= 0)
				throw new ArgumentException("precision must be a positive number");
			if (scale < 0 || scale >= precision)
				throw new ArgumentException("scale must be a non-negative number less than precision");
			numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
		}

		public bool IsValidNumber(string value)
		{
			// Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
			// описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
			// Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
			// целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
			// Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

			if (string.IsNullOrEmpty(value))
				return false;

			var match = numberRegex.Match(value);
			if (!match.Success)
				return false;

			// Знак и целая часть
			var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
			// Дробная часть
			var fracPart = match.Groups[4].Value.Length;

			if (intPart + fracPart > precision || fracPart > scale)
				return false;

			if (onlyPositive && match.Groups[1].Value == "-")
				return false;
			return true;
		}
	}
}