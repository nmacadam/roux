using System.Collections.Generic;

namespace Roux
{
    // todo: handle implicit semicolons better (will break once braces are introduced)

    /// <summary>
    /// Tokenizes Roux source code
    /// </summary>
    internal class Scanner
    {
        private readonly string _source;
        private readonly List<Token> _tokens = new List<Token>();

        private int _start = 0;
        private int _current = 0;
        private int _line = 1;

        private readonly IErrorReporter _errorReporter;

        /// <summary>
        /// Keyword mapping to token type
        /// </summary>
        private static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
        {
            {"and",         TokenType.And         },
            {"base",        TokenType.Base        },
            {"break",       TokenType.Break       },
            {"class",       TokenType.Class       },
            {"continue",    TokenType.Continue    },
            {"else",        TokenType.Else        },
            {"false",       TokenType.False       },
            {"for",         TokenType.For         },
            {"fun",         TokenType.Fun         },
            {"if",          TokenType.If          },
            {"null",        TokenType.Null        },
            {"operator",    TokenType.Operator    },
            {"or",          TokenType.Or          },
            {"private",     TokenType.Private     },
            {"protected",   TokenType.Protected   },
            {"public",      TokenType.Public      },
            {"print",       TokenType.Print       },
            {"return",      TokenType.Return      },
            {"static",      TokenType.Static      },
            {"this",        TokenType.This        },
            {"true",        TokenType.True        },
            {"var",         TokenType.Var         },
            {"while",       TokenType.While       }
        };

        public Scanner(string source, IErrorReporter errorReporter)
        {
            _source = source;
            _errorReporter = errorReporter;
        }

        /// <summary>
        /// Scan all tokens from source string
        /// </summary>
        /// <returns>The list of tokens collected from the source string</returns>
        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                _start = _current;
                ScanToken();
            }

            // Add a semicolon at the end so the REPL works with implicit semicolons
            if (_line == 1 && _tokens.Count > 0 && _tokens[_tokens.Count - 1].TokenType != TokenType.Semicolon)
            {
                //_tokens.Add(new Token(TokenType.Semicolon, "", null, _line));
            }

            _tokens.Add(new Token(TokenType.Eof, "", null, _line));
            return _tokens;
        }

        /// <summary>
        /// Scans an individual token
        /// </summary>
        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                // Single-character
                case '(': AddToken(TokenType.LeftParenthesis); break;
                case ')': AddToken(TokenType.RightParenthesis); break;
                case '{': AddToken(TokenType.LeftBrace); break;
                case '}': AddToken(TokenType.RightBrace); break;
                case '[': AddToken(TokenType.LeftBracket); break;
                case ']': AddToken(TokenType.RightBracket); break;
                case ',': AddToken(TokenType.Comma); break;
                case '.': AddToken(TokenType.Dot); break;
                case ';': AddToken(TokenType.Semicolon); break;
                case ':': AddToken(TokenType.Colon); break;
                case '?': AddToken(TokenType.QuestionMark); break;
                case '~': AddToken(TokenType.Tilde); break;

                // Single or double characters
                case '-':
                    if (Match('-')) AddToken(TokenType.MinusMinus);
                    else if (Match('=')) AddToken(TokenType.MinusEqual);
                    else AddToken(TokenType.Minus);
                    break;
                case '+':
                    if (Match('+')) AddToken(TokenType.PlusPlus);
                    else if (Match('=')) AddToken(TokenType.PlusEqual);
                    else AddToken(TokenType.Plus);
                    break;
                case '*': AddToken(Match('=') ? TokenType.StarEqual : TokenType.Star); break;
                case '%': AddToken(Match('=') ? TokenType.PercentEqual : TokenType.Percent); break;
                case '|': AddToken(Match('=') ? TokenType.BarEqual : TokenType.Bar); break;
                case '&': AddToken(Match('=') ? TokenType.AmpersandEqual : TokenType.Ampersand); break;
                case '^': AddToken(Match('=') ? TokenType.CaretEqual : TokenType.Caret); break;

                case '!': AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang); break;
                case '=': AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal); break;
                case '<': AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less); break;
                case '>': AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater); break;
                case '/':
                    if (Match('/'))
                    {
                        // A comment goes until the end of the line.                
                        while (Peek() != '\n' && !IsAtEnd()) Advance();

                        //Advance();
                    }
                    else if (Match('*'))
                    {
                        // A comment goes until the end of the block                
                        while (!IsAtEnd())
                        {
                            if (Peek() == '*' && PeekNext() == '/')
                            {
                                Advance();
                                Advance();
                                break;
                            }

                            Advance();
                        }
                    }
                    else if (Match('='))
                    {
                        AddToken(TokenType.SlashEqual);
                    }
                    else
                    {
                        AddToken(TokenType.Slash);
                    }
                    break;

                // Literals
                case '"': MakeString(); break;
                case '\'': MakeString('\''); break;

                // White-space
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;

                case '\n':
                    if (_tokens.Count > 0 && _tokens[_tokens.Count - 1].TokenType != TokenType.Semicolon)
                    {
                        //AddToken(TokenType.Semicolon);
                    }
                    _line++;
                    break;

                // If not a language token, determine what the user has entered and if it is valid
                default:
                    if (IsDigit(c))
                    {
                        MakeNumber();
                    }
                    else if (IsAlpha(c))
                    {
                        Identifier();
                    }
                    else
                    {
                        _errorReporter.Error(_line, "Unexpected character.");
                    }
                    break;
            }
        }

        /// <summary>
        /// Constructs a token with a string literal
        /// </summary>
        private void MakeString(char startingChar = '"')
        {
            while (Peek() != startingChar && !IsAtEnd())
            {
                if (Peek() == '\n') _line++;
                Advance();
            }

            // Unterminated string.                                 
            if (IsAtEnd())
            {
                _errorReporter.Error(_line, "Unterminated string.");
                return;
            }

            // The closing ".                                       
            Advance();

            // Trim the surrounding quotes.                         
            string value = _source.Substring(_start + 1, (_current - 1) - (_start + 1));
            AddToken(TokenType.String, value);
        }

        /// <summary>
        /// Constructs a token with a numeric literal
        /// </summary>
        private void MakeNumber()
        {
            while (IsDigit(Peek())) Advance();

            // Look for a fractional part.                            
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                // Consume the "."                                      
                Advance();

                while (IsDigit(Peek())) Advance();
            }

            AddToken(TokenType.Number,
                double.Parse(_source.Substring(_start, _current - _start)));
        }

        /// <summary>
        /// Constructs a token for a keyword if valid
        /// </summary>
        private void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            // See if the identifier is a reserved word.   
            string text = _source.Substring(_start, _current - _start);

            TokenType type;
            if (Keywords.ContainsKey(text))
            {
                type = Keywords[text];
            }
            else type = TokenType.Identifier;
            AddToken(type);
        }

        /// <summary>
        /// Return if the character is a letter
        /// </summary>
        /// <param name="c">Character to check</param>
        /// <returns>If the character is a letter</returns>
        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                    c == '_';
        }

        /// <summary>
        /// Return if the character is a digit
        /// </summary>
        /// <param name="c">Character to check</param>
        /// <returns>If the character is a digit</returns>
        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        /// <summary>
        /// Return if the character is alphanumeric
        /// </summary>
        /// <param name="c">Character to check</param>
        /// <returns>If the character is alphanumeric</returns>
        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        /// <summary>
        /// Conditionally advance if the expected character is the current character 
        /// </summary>
        /// <param name="expected">The expected next character</param>
        /// <returns>Whether the next character was the expected character</returns>
        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (_source[_current] != expected) return false;

            _current++;
            return true;
        }

        /// <summary>
        /// Return the current character in the source without consuming it
        /// </summary>
        /// <returns>The current character</returns>
        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return _source[_current];
        }

        /// <summary>
        /// Return the next character in the source without consuming it or the previous character
        /// </summary>
        /// <returns>The next character</returns>
        private char PeekNext()
        {
            if (_current + 1 >= _source.Length) return '\0';
            return _source[_current + 1];
        }

        /// <summary>
        /// Consumes the next character in the source and returns it
        /// </summary>
        /// <returns>The next character</returns>
        private char Advance()
        {
            _current++;
            return _source[_current - 1];
        }

        /// <summary>
        /// Creates a token for the current lexeme
        /// </summary>
        /// <param name="type">The TokenType to create a token for</param>
        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        /// <summary>
        /// Creates a token for the current lexeme, including a literal value
        /// </summary>
        /// <param name="type">The TokenType to create a token for</param>
        /// <param name="literal">The literal value of the token</param>
        private void AddToken(TokenType type, object literal)
        {
            string text = _source.Substring(_start, _current - _start);
            _tokens.Add(new Token(type, text, literal, _line));
        }

        /// <summary>
        /// Determine if the iterator has reached the end of the source string
        /// </summary>
        /// <returns>If the iterator has reached the end of the source string</returns>
        private bool IsAtEnd()
        {
            return _current >= _source.Length;
        }
    }
}