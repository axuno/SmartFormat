using System;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SmartFormat.Tests.TestUtils
{
    /// <summary>
    /// Generic class that can be used for United States addresses
    /// </summary>
    public class Address
    {
        public Address(string newStreetAddress, string newCity, States newState, string newZip)
        {
            StreetAddress = newStreetAddress;
            City = newCity;
            State = newState;
            Zip = newZip;
        }
        public Address(string newStreetAddress, string newCity, string newState, string newZip)
        {
            StreetAddress = newStreetAddress;
            City = newCity;
            State = ParseState(newState);
            Zip = newZip;
        }
        public override string ToString()
        {
            return FullAddress;
        }


        public string StreetAddress { get; set; }

        public string City { get; set; }

        public States State { get; set; }

        public string Zip { get; set; }


        /// <summary>
        /// Parses the AddressString to determine the Street Address, City, State, and Zip.
        /// Addresses are expected to be in 2-line style, or in single-line style with a comma between the address and city.
        ///
        /// Throws an exception if the address cannot be properly parsed!
        /// </summary>
        /// <param name="addressString">The entire Address.</param>
        public static bool TryParse(string addressString, ref Address result)
        {

            Match match = static_TryParse_AddressPattern.Match(addressString);
            if (!match.Success)
                return false;

            result = new Address(match.Groups["streetaddress"].Value, match.Groups["city"].Value, match.Groups["state"].Value, match.Groups["zip"].Value);
            return true;
        }
        static Regex static_TryParse_AddressPattern = new("(?<streetaddress>.*?)\\s*[\\n,]\\s*(?<city>.*?),\\s*(?<state>\\S\\S)\\s*(?<zip>\\S*)");
        public static Address Parse(string addressString)
        {
            Address result = new("","","","");
            if (!TryParse(addressString, ref result))
                throw new Exception("The Address String could not be properly parsed.");
            return result;
        }

        /// <summary>
        /// Returns the full, formatted address on two lines.
        /// </summary>
        public string FullAddress {
            get
            {
                string format = "{0}\n{1}, {2} {3}";
                return string.Format(format, this.StreetAddress, this.City, this.StateAbbreviation, this.Zip).Replace("\n", "\r\n");
            }
        }

        #region " State Abbreviations "

        /// <summary>
        /// Returns the 2-letter abbreviation of the state.
        /// For example, States.California = "CA"
        /// </summary>
        public string StateAbbreviation {
            get { return GetStateAbbreviation(this.State); }
        }
        public static string GetStateAbbreviation(States state)
        {
            return AbbreviationAttribute.GetAbbreviation(state);
        }
        public static States ParseState(string state)
        {
            // See if the abbreviation matches one of the states:
            States result = States.Unknown;
            if (AbbreviationAttribute.TryFindAbbreviation<States>(state, true, ref result))
                return result;

            // Try to parse the full state name:
            try {
                return (States)Enum.Parse(typeof(States), state, true);
            } catch {
                // Couldn't parse the full state name!
                return States.Unknown;
            }

        }
        #endregion

    }

    public enum States
    {
        Unknown = -1,
        [Abbreviation("AL")]
        Alabama,
        [Abbreviation("AK")]
        Alaska,
        [Abbreviation("AZ")]
        Arizona,
        [Abbreviation("AR")]
        Arkansas,
        [Abbreviation("CA")]
        California,
        [Abbreviation("CO")]
        Colorado,
        [Abbreviation("CT")]
        Connecticut,
        [Abbreviation("DE")]
        Delaware,
        [Abbreviation("FL")]
        Florida,
        [Abbreviation("GA")]
        Georgia,
        [Abbreviation("GU")]
        Guam,
        [Abbreviation("HI")]
        Hawaii,
        [Abbreviation("ID")]
        Idaho,
        [Abbreviation("IL")]
        Illinois,
        [Abbreviation("IN")]
        Indiana,
        [Abbreviation("IA")]
        Iowa,
        [Abbreviation("KS")]
        Kansas,
        [Abbreviation("KY")]
        Kentucky,
        [Abbreviation("LA")]
        Louisiana,
        [Abbreviation("ME")]
        Maine,
        [Abbreviation("MD")]
        Maryland,
        [Abbreviation("MA")]
        Massachusetts,
        [Abbreviation("MI")]
        Michigan,
        [Abbreviation("MN")]
        Minnesota,
        [Abbreviation("MS")]
        Mississippi,
        [Abbreviation("MO")]
        Missouri,
        [Abbreviation("MT")]
        Montana,
        [Abbreviation("NE")]
        Nebraska,
        [Abbreviation("NV")]
        Nevada,
        [Abbreviation("NH")]
        NewHampshire,
        [Abbreviation("NJ")]
        NewJersey,
        [Abbreviation("NM")]
        NewMexico,
        [Abbreviation("NY")]
        NewYork,
        [Abbreviation("NC")]
        NorthCarolina,
        [Abbreviation("ND")]
        NorthDakota,
        [Abbreviation("OH")]
        Ohio,
        [Abbreviation("OK")]
        Oklahoma,
        [Abbreviation("OR")]
        Oregon,
        [Abbreviation("PA")]
        Pennsylvania,
        [Abbreviation("RI")]
        RhodeIsland,
        [Abbreviation("SC")]
        SouthCarolina,
        [Abbreviation("SD")]
        SouthDakota,
        [Abbreviation("TN")]
        Tennessee,
        [Abbreviation("TX")]
        Texas,
        [Abbreviation("UT")]
        Utah,
        [Abbreviation("VT")]
        Vermont,
        [Abbreviation("VA")]
        Virginia,
        [Abbreviation("WA")]
        Washington,
        [Abbreviation("WV")]
        WestVirginia,
        [Abbreviation("WI")]
        Wisconsin,
        [Abbreviation("WY")]
        Wyoming,

        [Abbreviation("AS")]
        AmericanSamoa,
        [Abbreviation("DC")]
        DistrictOfColumbia,
        [Abbreviation("FM")]
        FederatedStatesOfMicronesia,
        [Abbreviation("MH")]
        MarshallIslands,
        [Abbreviation("MP")]
        NorthernMarianaIslands,
        [Abbreviation("PW")]
        Palau,
        [Abbreviation("PR")]
        PuertoRico,
        [Abbreviation("VI")]
        VirginIslands,
        [Abbreviation("AE")]
        ArmedForcesAfrica,
        [Abbreviation("AA")]
        ArmedForcesAmericasExceptCanada,
        [Abbreviation("AE")]
        ArmedForcesCanada,
        [Abbreviation("AE")]
        ArmedForcesEurope,
        [Abbreviation("AE")]
        ArmedForcesMiddleEast,
        [Abbreviation("AP")]
        ArmedForcesPacific
    }

    /// <summary>
    /// Allows you to specify an abbreviation for an item.  Useful for Enumerations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class AbbreviationAttribute : Attribute
    {
        public readonly string Abbreviation;
        public AbbreviationAttribute(string newAbbreviation)
        {
            this.Abbreviation = newAbbreviation;
        }

        /// <summary>
        /// Returns the abbreviation from an item that should be marked with the AbbreviationAttribute.
        ///
        /// For example, GetAbbreviation(States.California) = "CA"
        /// </summary>
        public static string GetAbbreviation(object value)
        {
            Type baseType = value.GetType();
            FieldInfo? fieldInfo = baseType.GetField(Enum.GetName(baseType, value)!);
            foreach (AbbreviationAttribute abbr in fieldInfo!.GetCustomAttributes(typeof(AbbreviationAttribute), true)) {
                return abbr.Abbreviation;
            }
            // Couldn't find anything:
            return "";
        }

        /// <summary>
        /// Searches for the object from the abbreviation
        /// </summary>
        public static bool TryFindAbbreviation<TBaseType>(string abbreviation, bool ignoreCase, ref TBaseType result)
        {
            // Search for the abbreviation:
            foreach (FieldInfo f in typeof(TBaseType).GetFields()) {
                foreach (AbbreviationAttribute abbr in f.GetCustomAttributes(typeof(AbbreviationAttribute), true)) {
                    if (string.Compare(abbreviation, abbr.Abbreviation, ignoreCase) == 0) {
                        result = (TBaseType)f.GetValue(null)!;
                        return true;
                    }
                }
            }
            return false;
        }

    }
}