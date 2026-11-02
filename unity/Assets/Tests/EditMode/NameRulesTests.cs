using FamilyQuiz.Core.Rules;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>PROTOCOL.md section 3 "Names", docs/18_edge_cases.md (names with emoji, symbols, 17 characters).</summary>
    public sealed class NameRulesTests
    {
        [TestCase("Mama", "Mama")]
        [TestCase("  Мама  ", "Мама")]
        [TestCase("Ёжик ёлка", "Ёжик ёлка")]
        [TestCase("Anna 2", "Anna 2")]
        [TestCase("ЭЮЯ эюя АБВ", "ЭЮЯ эюя АБВ")]
        [TestCase("A", "A")]
        [TestCase("abcdefghijklmnop", "abcdefghijklmnop")] // 16
        [TestCase("  абвгдежзийклмно  ", "абвгдежзийклмно")]
        public void AllowedNamesAreTrimmedAndKept(string raw, string expected)
        {
            Assert.That(NameRules.Normalize(raw), Is.EqualTo(expected));
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        [TestCase("abcdefghijklmnopq")] // 17
        [TestCase("Mama\U0001F600")] // emoji
        [TestCase("Ma-ma")]
        [TestCase("Mama!")]
        [TestCase("Dad_1")]
        [TestCase("Zoë")] // Latin letter outside A to Z
        [TestCase("Ґанна")] // Cyrillic outside А to я, Ё, ё
        [TestCase("Ma\tma")]
        public void OtherNamesAreRefused(string raw)
        {
            Assert.That(NameRules.Normalize(raw), Is.Null);
        }

        [TestCase("Mama", "mama")]
        [TestCase("мама", "МАМА")]
        [TestCase("ёж", "ЁЖ")]
        public void SameNameIgnoresCase(string a, string b)
        {
            Assert.That(NameRules.SameName(a, b), Is.True);
        }
    }
}
