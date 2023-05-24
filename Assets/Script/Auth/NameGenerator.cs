using System;
using System.Text;

namespace Script.Auth
{
    public static class NameGenerator
    {
        public static string GetName(string userId)
        {
            int hash = userId.GetHashCode();
            hash *= Math.Sign(hash);
            StringBuilder nameOutput = new StringBuilder();
            string[] words = new string[]
            {
                "Ant", "Bear", "Crow", "Dog", "Eel", "Frog", "Gopher", "Heron", "Ibex", "Jerboa", "Koala", "Llama", "Moth", "Newt", "Owl", "Puffin", "Rabbit", "Snake", "Trout", "Vulture", "Wolf"
            };
            
            nameOutput.Append(words[hash % words.Length]);
            int number = hash % 1000;
            nameOutput.Append(number.ToString("000"));
            
            return nameOutput.ToString();
        }
    }
}