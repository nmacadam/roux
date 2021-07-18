# roux
Roux is a WIP dynamically typed, interpreted scripting language (and interpreter written in C#!) based on Bob Nystrom's *lox* language from [*Crafting Interpreters*](https://craftinginterpreters.com/).

## 📝 examples
```javascript
// fizzbuzz
fun fizzbuzz(start, end)
{
    for (var i = 0; i < end; i++)
    {
        if (i % 3 == 0 and i % 5 == 0) print "FizzBuzz";
        else if (i % 3 == 0) print "Fizz";
        else if (i % 5 == 0) print "Buzz";
        else print i;
    }
}

fizzbuzz(1,15);
```

## changes to the lox language
- more operators (+=, -=, *=, /=, ++, --, ?:. comma, and more...)
- implicit semicolons
- WIP
