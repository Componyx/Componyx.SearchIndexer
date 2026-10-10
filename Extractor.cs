using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Componyx.Search.Indexer
{
    public class Extractor
    {
        private static Regex non1 = new Regex(@"(^|\s)(a|aboard|about|above|absent|according\sto|across|after|against|ago|ahead\sof|ain't|all|along|alongside)(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private static Regex non2 = new Regex(@"(^|\s)(also|although|am|amid|amidst|among|amongst|an|and|anti|anybody|anyone|anything|apart|apart\sfrom|are|been)(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private static Regex non3 = new Regex(@"(^|\s)(aren't|around|as|as\sfar\sas|as\ssoon\sas|as\swell\sas|aside|at|atop|away|be|because|because\sof|before)(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private static Regex non4 = new Regex(@"(^|\s)(behind|below|beneath|beside|besides|between|betwixt|beyond|but|by|by\smeans\sof|by\sthe\stime|can|cannot)(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private static Regex non5 = new Regex(@"(^|\s)(circa|close\sto|com|concerning|considering|could|couldn't|cum|'d|despite|did|didn't|do|does|doesn't|don't)(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private static Regex non6 = new Regex(@"(^|\s)(down|due\sto|during|each|'em|either|even|ever|every|every\stime|everybody|everyone)(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private static Regex non7 = new Regex(@"(^|\s)(everything|except|far\sfrom|few|first\stime|following|for|from|further|get|got|had|hadn't|has|hasn't|have)(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private static Regex non8 = new Regex(@"(^|\s)(haven't|he|hence|her|here|hers|herself|him|himself|his|how|i|if|in|in\saccordance\swith|in\saddition\sto|in\scase)(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private static Regex non9 = new Regex(@"(^|\s)(in\sfront\sof|in\slieu\sof|in\splace\sof|in\sspite\sof|in\sthe\sevent\sthat|in\sto|inside|inside\sof)(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private static Regex non10 = new Regex(@"(^|\s)(instead\sof|into|is|isn't|it|itself|just\sin\scase|like|'ll|lots|may|me|mid|might|mightn't|mine|more|most)(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private static Regex non11 = new Regex(@"(^|\s)(must|mustn't|myself|near|near\sto|nearest|new|no|no\sone|nobody|none|not|nothing|notwithstanding|now\sthat|of)(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private static Regex non12 = new Regex(@"(^|\s)(off|on|on\sbehalf\sof|on\sto|on\stop\sof|once|one|one\sanother|only\sif|onto|opposite|or|org|other|our|any)(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private static Regex non13 = new Regex(@"(^|\s)(ours|ourselves|out|out\sof|outside|outside\sof|over|past|per|plenty|plus|prior\sto|qua|re|'re|really|set)(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private static Regex non14 = new Regex(@"(^|\s)(regarding|round|'s|said|sans|save|say|says|shall|shan't|she|should|shouldn't|since|so|somebody|its|only)(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private static Regex non15 = new Regex(@"(^|\s)(someone|something|than|that|the|thee|their|theirs|them|themselves|there|these|they|thine|this|thou)(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private static Regex non16 = new Regex(@"(^|\s)(though|through|throughout|till|to|too|toward|towards|under|underneath|unless|unlike|until|unto|up|upon|using|even)(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private static Regex non17 = new Regex(@"(^|\s)(us|'ve|versus|via|was|wasn't|we|were|weren't|well|what|when|whenever|where|whereas|whether\sor\snot|things)(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private static Regex non18 = new Regex(@"(^|\s)(which|while|who|whoever|whom|why|will|with|with\sregard\sto|withal|within|without|won't|would|wouldn't|mere)(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private static Regex non19 = new Regex(@"(^|\s)(ya|ye|yes|you|your|yours|yourself)(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));

        // Additional common stopwords not covered by non1-non19.
        private static Regex non20 = new Regex(@"(^|\s)(then|now|yet|however|such|same|own|much|many|still|already|always|never|etc)(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));

        // Punctuation other than apostrophes, stripped early so ordinary words
        // sitting directly against a comma, period, etc. still get whitespace
        // boundaries for regone/regtwo/non1-non20 to match against.
        private static Regex special1 = new Regex(@"\:|\;|\.|\,|\(|\)|\[|\]|\{|\}|\""", RegexOptions.Compiled);

        // Apostrophes are deliberately NOT included above. Stripping them
        // early turns "didn't" into "didn" + "t" before non5 ever gets a
        // chance to match the literal "didn't" pattern, so the whole
        // contraction never matches, and the "didn" fragment leaks into the
        // index as a keyword. Apostrophes are stripped separately, after
        // non1-non20 have already run, so contractions match intact first.
        private static Regex apostrophe = new Regex(@"\'", RegexOptions.Compiled);

        private static Regex special2 = new Regex(@"(^|\s)[^a-zA-Z]+(\s|$)", RegexOptions.Compiled);
        private static Regex html = new Regex(@"(\&+\S*\;)", RegexOptions.Compiled);
        private static Regex numbers = new Regex(@"(^|\s)[\d]+(\S*)(\s|$)", RegexOptions.Compiled);
        private static Regex line = new Regex(@"\n+|\r+", RegexOptions.Compiled);
        private static Regex regone = new Regex(@"(^|\s)([^\s]{1})(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private static Regex regtwo = new Regex(@"(^|\s)(am|an|as|at|ax|be|by|do|go|he|if|in|is|it|me|my|no|of|on|or|ox|so|to|up|us|we|ie|eg)(\s|$)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private static Regex space = new Regex(@"\s+", RegexOptions.Compiled);
        private static Regex singleSpace = new Regex(@"\s", RegexOptions.Compiled);

        public static string[] Extract(string text)
        {
            return Filter(text).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static string Filter(string text)
        {
            text = line.Replace(text, "   "); // replace with first space, replacement, second space
            text = singleSpace.Replace(text, "  ");  // because we do not use word-boundary \b but instead use \sword\s we need to make sure that the next word is grabbed also by adding extra space between words
            text = numbers.Replace(text, "   ");
            text = html.Replace(text, "   ");
            text = special2.Replace(text, "   ");
            text = special1.Replace(text, "   ");
            text = regone.Replace(text, "   ");
            text = regtwo.Replace(text, "   ");
            text = non1.Replace(text, "   ");
            text = non2.Replace(text, "   ");
            text = non3.Replace(text, "   ");
            text = non4.Replace(text, "   ");
            text = non5.Replace(text, "   ");
            text = non6.Replace(text, "   ");
            text = non7.Replace(text, "   ");
            text = non8.Replace(text, "   ");
            text = non9.Replace(text, "   ");
            text = non10.Replace(text, "  ");
            text = non11.Replace(text, "   ");
            text = non12.Replace(text, "   ");
            text = non13.Replace(text, "   ");
            text = non14.Replace(text, "   ");
            text = non15.Replace(text, "   ");
            text = non16.Replace(text, "   ");
            text = non17.Replace(text, "   ");
            text = non18.Replace(text, "   ");
            text = non19.Replace(text, "   ");
            text = non20.Replace(text, "   ");
            text = apostrophe.Replace(text, "   "); // leftover apostrophes, contractions not on the list above
            text = regone.Replace(text, "   "); // re-run: the line above can produce new single-character fragments
            text = space.Replace(text, " ");
            return text;
        }
    }
}
