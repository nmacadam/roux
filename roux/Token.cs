namespace Roux
{
    internal enum TokenType
    {
        // Single-character tokens
        LeftParenthesis, RightParenthesis,
        LeftBrace, RightBrace,
        LeftBracket, RightBracket,
        Comma, Dot, Minus, Plus, Semicolon, Colon, QuestionMark, Slash, Star, Percent, Tilde,

        // One or two character tokens
        Ampersand, AmpersandEqual, Bar, BarEqual, Caret, CaretEqual,
        Bang, BangEqual, Equal, EqualEqual, Greater, GreaterEqual, Less, LessEqual,
        PlusEqual, MinusEqual, SlashEqual, StarEqual, PercentEqual,
        PlusPlus, MinusMinus,

        // Literals
        Identifier, String, Number,

        // Keywords
        And, Base, Break, Class, Construct, Continue, Else, False, For, Fun, If, Null, 
        Operator, Or, Private, Protected, Public, Print, Return, Static, This, True, 
        Var, While, 
        
        Eof
    }

    /// <summary>
    /// Represents an individual character (or character(s)) valid within the Roux interpreter 
    /// </summary>
    internal class Token
    {
        public readonly TokenType TokenType;
        public readonly string Lexeme;
        public readonly object Literal;
        public readonly int Line;

        public Token(TokenType type, string lexeme, object literal, int line)
        {
            TokenType = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = line;
        }

        public override string ToString()
        {
            return $"{TokenType} {Lexeme} {Literal}";
        }

        public Token Copy(TokenType newToken, string newLexeme = "")
        {
            return new Token(newToken, string.IsNullOrEmpty(newLexeme) ? Lexeme : newLexeme, Literal, Line);
        }
    }
}