using NUnit.Framework;
using Roux;
using System.Collections.Generic;

namespace RouxTests
{
    class LanguageIntegrationTests
    {
        private TestErrorReporter _errorReporter = new TestErrorReporter();

        [SetUp]
        public void Setup()
        {
            _errorReporter.Reset();
        }

        private T GetExpressionResult<T>(string input)
        {
            Scanner scanner = new Scanner(input, _errorReporter);
            List<Token> tokens = scanner.ScanTokens();

            Parser parser = new Parser(tokens, _errorReporter);
            Expr expression = parser.ParseExpression();

            Interpreter interpreter = new Interpreter(_errorReporter);
            object value = interpreter.InterpretExpression(expression);

            return (T)value;
        }

        #region Expressions

        [Test]
        public void CommaOperator()
        {
            Assert.AreEqual(2.0, GetExpressionResult<double>("1, 2"));
        }

        [Test]
        public void TernaryOperator()
        {
            Assert.AreEqual(1.0, GetExpressionResult<double>("true ? 1 : 2"));
            Assert.AreEqual(2.0, GetExpressionResult<double>("false ? 1 : 2"));
        }

        [Test]
        public void ComparisonOperators()
        {
            Assert.AreEqual(true, GetExpressionResult<bool>("1 < 2"));
            Assert.AreEqual(true, GetExpressionResult<bool>("1 <= 2"));
            Assert.AreEqual(true, GetExpressionResult<bool>("2 <= 2"));
            Assert.AreEqual(true, GetExpressionResult<bool>("2 > 1"));
            Assert.AreEqual(true, GetExpressionResult<bool>("2 >= 1"));
            Assert.AreEqual(true, GetExpressionResult<bool>("2 >= 2"));
        }

        [Test]
        public void EqualityOperators()
        {
            Assert.AreEqual(true, GetExpressionResult<bool>("1 == 1"));
            Assert.AreEqual(true, GetExpressionResult<bool>("1 != 2"));
        }

        [Test]
        public void BitwiseOperators()
        {
            Assert.Fail("Not implemented");
        }

        [Test]
        public void TermOperators()
        {
            Assert.AreEqual(2.0, GetExpressionResult<double>("1 + 1"));
            Assert.AreEqual(0.0, GetExpressionResult<double>("1 - 1"));
        }

        [Test]
        public void FactorOperators()
        {
            Assert.AreEqual(4.0, GetExpressionResult<double>("2 * 2"));
            Assert.AreEqual(2.0, GetExpressionResult<double>("4 / 2"));
            Assert.AreEqual(0.0, GetExpressionResult<double>("4 % 2"));
        }

        [Test]
        public void UnaryOperators()
        {
            Assert.AreEqual(-1.0, GetExpressionResult<double>("-1"));
            Assert.AreEqual(false, GetExpressionResult<bool>("!true"));
        }

        [Test]
        public void PrimaryExpressions()
        {
            Assert.AreEqual(1.0, GetExpressionResult<double>("1"));
            Assert.AreEqual("hello world", GetExpressionResult<string>("'hello world'"));
            Assert.AreEqual(true, GetExpressionResult<bool>("true"));
            Assert.AreEqual(false, GetExpressionResult<bool>("false"));
            Assert.AreEqual(null, GetExpressionResult<object>("null"));
        }

        [Test]
        public void FollowsOperatorPrecedence()
        {
            // * has higher precedence than +.
            Assert.AreEqual(14.0, GetExpressionResult<double>("2 + 3 * 4"));

            // * has higher precedence than -.
            Assert.AreEqual(8.0, GetExpressionResult<double>("20 - 3 * 4"));

            // / has higher precedence than +.
            Assert.AreEqual(4.0, GetExpressionResult<double>("2 + 6 / 3"));

            // / has higher precedence than -.
            Assert.AreEqual(0.0, GetExpressionResult<double>("2 - 6 / 3"));

            // todo: bitwise

            // < has higher precedence than ==.
            Assert.AreEqual(true, GetExpressionResult<bool>("false == 2 < 1"));

            // > has higher precedence than ==.
            Assert.AreEqual(true, GetExpressionResult<bool>("false == 1 > 2"));

            // <= has higher precedence than ==.
            Assert.AreEqual(true, GetExpressionResult<bool>("false == 2 <= 1"));

            // >= has higher precedence than ==.
            Assert.AreEqual(true, GetExpressionResult<bool>("false == 1 >= 2"));

            // ?: has a higher precedence than >
            Assert.AreEqual(false, GetExpressionResult<bool>("1 > 2 ? true : false"));

            // ?: has a higher precedence than >=
            Assert.AreEqual(false, GetExpressionResult<bool>("1 >= 2 ? true : false"));

            // ?: has a higher precedence than <
            Assert.AreEqual(false, GetExpressionResult<bool>("2 < 1 ? true : false"));

            // ?: has a higher precedence than <=
            Assert.AreEqual(false, GetExpressionResult<bool>("2 <= 1 ? true : false"));

            // , has a higher precedence than ?:
            Assert.AreEqual(false, GetExpressionResult<bool>("true, false ? true : false"));

            // 1 - 1 is not space-sensitive.
            Assert.AreEqual(0.0, GetExpressionResult<double>("1 - 1"));
            Assert.AreEqual(0.0, GetExpressionResult<double>("1 -1"));
            Assert.AreEqual(0.0, GetExpressionResult<double>("1- 1"));
            Assert.AreEqual(0.0, GetExpressionResult<double>("1-1"));

            // Using () for grouping.
            Assert.AreEqual(4.0, GetExpressionResult<double>("(2 * (6 - (2 + 2)))"));
        }

        #endregion


    }
}
