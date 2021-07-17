using System.Collections.Generic;
using NUnit.Framework;
using Roux;

namespace RouxTests
{
    public class ScannerTests
    {
        private TestErrorReporter _errorReporter;

        [SetUp]
        public void Setup()
        {
            _errorReporter = new TestErrorReporter();
        }

        private void AssertTokenListValid(List<Token> tokens, int expectedSize = 1)
        {
            Assert.IsNotNull(tokens);
            Assert.IsNotEmpty(tokens);
            Assert.AreEqual(expectedSize + 1, tokens.Count);
        }

        #region Scanning

        [Test]
        public void IgnoresLineComments()
        {
            string input = "//content\n123";
            Scanner scanner = new Scanner(input, _errorReporter);
            List<Token> tokens = scanner.ScanTokens();

            AssertTokenListValid(tokens, 1);
            Assert.IsTrue(tokens[0].TokenType == TokenType.Number);
            Assert.AreEqual("123", tokens[0].Lexeme);
        }

        [Test]
        public void IgnoresBlockComments()
        {
            string input = "/*content\nmore content*/123";
            Scanner scanner = new Scanner(input, _errorReporter);
            List<Token> tokens = scanner.ScanTokens();

            AssertTokenListValid(tokens, 1);
            Assert.IsTrue(tokens[0].TokenType == TokenType.Number);
            Assert.AreEqual("123", tokens[0].Lexeme);
        }

        [Test]
        public void IgnoresNonNewlineWhiteSpace()
        {
            AssertInputMatchesToken(" ", TokenType.Eof, 0);
            AssertInputMatchesToken("\r", TokenType.Eof, 0);
            AssertInputMatchesToken("\t", TokenType.Eof, 0);
        }

        [Test]
        public void IgnoresNewlineWhenPreviousTokenWasSemicolon()
        {
            string input = "a;\nb";
            Scanner scanner = new Scanner(input, _errorReporter);
            List<Token> tokens = scanner.ScanTokens();

            AssertTokenListValid(tokens, 3);
        }

        [Test]
        public void CallsErrorReporterOnInvalidCharacter()
        {
            Assert.Throws<AssertionException>(() =>
            {
                string input = "§";
                Scanner scanner = new Scanner(input, _errorReporter);
                List<Token> tokens = scanner.ScanTokens();
            });
        }

        [Test]
        public void CallsErrorReporterOnMismatchedStringQuotes()
        {
            Assert.Throws<AssertionException>(() =>
            {
                string input = "'hello\"";
                Scanner scanner = new Scanner(input, _errorReporter);
                List<Token> tokens = scanner.ScanTokens();
            });
        }

        #endregion

        #region Token Recognition

        private void AssertInputMatchesToken(string input, TokenType expected, int expectedListSize = 1)
        {
            Scanner scanner = new Scanner(input, _errorReporter);
            List<Token> tokens = scanner.ScanTokens();

            AssertTokenListValid(tokens, expectedListSize);
            Assert.IsTrue(tokens[0].TokenType == expected);
        }

        [Test]
        public void PlacesEof()
        {
            string input = "";
            Scanner scanner = new Scanner(input, _errorReporter);
            List<Token> tokens = scanner.ScanTokens();

            AssertTokenListValid(tokens, 0);
            Assert.IsTrue(tokens[0].TokenType == TokenType.Eof);
        }

        [Test]
        public void RecognizesParenthesis()
        {
            string input = "()";
            Scanner scanner = new Scanner(input, _errorReporter);
            List<Token> tokens = scanner.ScanTokens();

            AssertTokenListValid(tokens, 2);
            Assert.IsTrue(tokens[0].TokenType == TokenType.LeftParenthesis);
            Assert.IsTrue(tokens[1].TokenType == TokenType.RightParenthesis);
        }

        [Test]
        public void RecognizesBraces()
        {
            string input = "{}";
            Scanner scanner = new Scanner(input, _errorReporter);
            List<Token> tokens = scanner.ScanTokens();

            AssertTokenListValid(tokens, 2);
            Assert.IsTrue(tokens[0].TokenType == TokenType.LeftBrace);
            Assert.IsTrue(tokens[1].TokenType == TokenType.RightBrace);
        }

        [Test]
        public void RecognizesBrackets()
        {
            string input = "[]";
            Scanner scanner = new Scanner(input, _errorReporter);
            List<Token> tokens = scanner.ScanTokens();

            AssertTokenListValid(tokens, 2);
            Assert.IsTrue(tokens[0].TokenType == TokenType.LeftBracket);
            Assert.IsTrue(tokens[1].TokenType == TokenType.RightBracket);
        }

        [Test]
        public void RecognizesComma()
        {
            AssertInputMatchesToken(",", TokenType.Comma);
        }

        [Test]
        public void RecognizesDot()
        {
			AssertInputMatchesToken(".", TokenType.Dot);
        }

        [Test]
        public void RecognizesMinus()
        {
            AssertInputMatchesToken("-", TokenType.Minus);
        }

        [Test]
        public void RecognizesPlus()
        {
			AssertInputMatchesToken("+", TokenType.Plus);
        }

        [Test]
        public void RecognizesSemicolon()
        {
			AssertInputMatchesToken(";", TokenType.Semicolon);
        }

        [Test]
        public void RecognizesNewlineAfterNonSemicolonTokenAsSemicolon()
        {
            Scanner scanner = new Scanner("a\n", _errorReporter);
            List<Token> tokens = scanner.ScanTokens();

            AssertTokenListValid(tokens, 2);
            Assert.IsTrue(tokens[1].TokenType == TokenType.Semicolon);
        }

        [Test]
        public void RecognizesColon()
        {
			AssertInputMatchesToken(":", TokenType.Colon);
        }

        [Test]
        public void RecognizesQuestionMark()
        {
			AssertInputMatchesToken("?", TokenType.QuestionMark);
        }

        [Test]
        public void RecognizesSlash()
        {
			AssertInputMatchesToken("/", TokenType.Slash);
        }

        [Test]
        public void RecognizesStar()
        {
			AssertInputMatchesToken("*", TokenType.Star);
        }

        [Test]
        public void RecognizesPercent()
        {
			AssertInputMatchesToken("%", TokenType.Percent);
        }

        [Test]
        public void RecognizesBang()
        {
			AssertInputMatchesToken("!", TokenType.Bang);
        }

        [Test]
        public void RecognizesTilde()
        {
            AssertInputMatchesToken("~", TokenType.Tilde);
        }

        [Test]
        public void RecognizesAmpersand()
        {
            AssertInputMatchesToken("&", TokenType.Ampersand);
        }

        [Test]
        public void RecognizesBar()
        {
            AssertInputMatchesToken("|", TokenType.Bar);
        }

        [Test]
        public void RecognizesCaret()
        {
            AssertInputMatchesToken("^", TokenType.Caret);
        }

        [Test]
        public void RecognizesAmpersandEqual()
        {
            AssertInputMatchesToken("&=", TokenType.AmpersandEqual);
        }

        [Test]
        public void RecognizesBarEqual()
        {
            AssertInputMatchesToken("|=", TokenType.BarEqual);
        }

        [Test]
        public void RecognizesCaretEqual()
        {
            AssertInputMatchesToken("^=", TokenType.CaretEqual);
        }

        [Test]
        public void RecognizesBangEqual()
        {
			AssertInputMatchesToken("!=", TokenType.BangEqual);
        }

        [Test]
        public void RecognizesEqual()
        {
			AssertInputMatchesToken("=", TokenType.Equal);
        }

        [Test]
        public void RecognizesEqualEqual()
        {
			AssertInputMatchesToken("==", TokenType.EqualEqual);
        }

        [Test]
        public void RecognizesGreater()
        {
			AssertInputMatchesToken(">", TokenType.Greater);
        }

        [Test]
        public void RecognizesGreaterEqual()
        {
			AssertInputMatchesToken(">=", TokenType.GreaterEqual);
        }

        [Test]
        public void RecognizesLess()
        {
			AssertInputMatchesToken("<", TokenType.Less);
        }

        [Test]
        public void RecognizesLessEqual()
        {
			AssertInputMatchesToken("<=", TokenType.LessEqual);
        }

        [Test]
        public void RecognizesPlusEqual()
        {
			AssertInputMatchesToken("+=", TokenType.PlusEqual);
        }

        [Test]
        public void RecognizesMinusEqual()
        {
			AssertInputMatchesToken("-=", TokenType.MinusEqual);
        }

        [Test]
        public void RecognizesSlashEqual()
        {
			AssertInputMatchesToken("/=", TokenType.SlashEqual);
        }

        [Test]
        public void RecognizesStarEqual()
        {
			AssertInputMatchesToken("*=", TokenType.StarEqual);
        }

        [Test]
        public void RecognizesPercentEqual()
        {
			AssertInputMatchesToken("%=", TokenType.PercentEqual);
        }

        [Test]
        public void RecognizesPlusPlus()
        {
			AssertInputMatchesToken("++", TokenType.PlusPlus);
        }

        [Test]
        public void RecognizesMinusMinus()
        {
			AssertInputMatchesToken("--", TokenType.MinusMinus);
        }

        [Test]
        public void RecognizesIdentifier()
        {
            string input = "something";
            Scanner scanner = new Scanner(input, _errorReporter);
            List<Token> tokens = scanner.ScanTokens();

            AssertTokenListValid(tokens);
            Assert.IsTrue(tokens[0].TokenType == TokenType.Identifier);
        }

        [Test]
        public void RecognizesString()
        {
			AssertInputMatchesToken("\"hello\"", TokenType.String);
            AssertInputMatchesToken("'hello'", TokenType.String);
        }

        [Test]
        public void RecognizesNumber()
        {
			AssertInputMatchesToken("123", TokenType.Number);
        }

        [Test]
        public void RecognizesKeywordAnd()
        {
			AssertInputMatchesToken("and", TokenType.And);
        }

        [Test]
        public void RecognizesKeywordBase()
        {
            AssertInputMatchesToken("base", TokenType.Base);
        }

        [Test]
        public void RecognizesKeywordBreak()
        {
			AssertInputMatchesToken("break", TokenType.Break);
        }

        [Test]
        public void RecognizesKeywordClass()
        {
			AssertInputMatchesToken("class", TokenType.Class);
        }

        [Test]
        public void RecognizesKeywordConstruct()
        {
            AssertInputMatchesToken("construct", TokenType.Construct);
        }

        [Test]
        public void RecognizesKeywordContinue()
        {
			AssertInputMatchesToken("continue", TokenType.Continue);
        }

        [Test]
        public void RecognizesKeywordElse()
        {
			AssertInputMatchesToken("else", TokenType.Else);
        }

        [Test]
        public void RecognizesKeywordFalse()
        {
			AssertInputMatchesToken("false", TokenType.False);
        }

        [Test]
        public void RecognizesKeywordFor()
        {
			AssertInputMatchesToken("for", TokenType.For);
        }

        [Test]
        public void RecognizesKeywordIf()
        {
			AssertInputMatchesToken("if", TokenType.If);
        }

        [Test]
        public void RecognizesKeywordNulll()
        {
			AssertInputMatchesToken("null", TokenType.Null);
        }

        [Test]
        public void RecognizesKeywordOr()
        {
			AssertInputMatchesToken("or", TokenType.Or);
        }

        [Test]
        public void RecognizesKeywordOperator()
        {
            AssertInputMatchesToken("operator", TokenType.Operator);
        }

        [Test]
        public void RecognizesKeywordPrivate()
        {
            AssertInputMatchesToken("private", TokenType.Private);
        }

        [Test]
        public void RecognizesKeywordProtected()
        {
            AssertInputMatchesToken("protected", TokenType.Protected);
        }

        [Test]
        public void RecognizesKeywordPublic()
        {
            AssertInputMatchesToken("public", TokenType.Public);
        }

        [Test]
        public void RecognizesKeywordPrint()
        {
			AssertInputMatchesToken("print", TokenType.Print);
        }

        [Test]
        public void RecognizesKeywordReturn()
        {
			AssertInputMatchesToken("return", TokenType.Return);
        }

        [Test]
        public void RecognizesKeywordStatic()
        {
            AssertInputMatchesToken("static", TokenType.Static);
        }

        [Test]
        public void RecognizesKeywordThis()
        {
			AssertInputMatchesToken("this", TokenType.This);
        }

        [Test]
        public void RecognizesKeywordTrue()
        {
			AssertInputMatchesToken("true", TokenType.True);
        }

        [Test]
        public void RecognizesKeywordVar()
        {
			AssertInputMatchesToken("var", TokenType.Var);
        }

        [Test]
        public void RecognizesKeywordWhile()
        {
			AssertInputMatchesToken("while", TokenType.While);
        }

        #endregion
    }
}