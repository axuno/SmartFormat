// 
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
// 

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SmartFormat.Utilities
{
    /// <summary>
    /// The class contains <see cref="TimeTextInfo"/> definitions for common languages.
    /// </summary>
    public static class CommonLanguagesTimeTextInfo
    {
        private static readonly Dictionary<string, TimeTextInfo> _customLanguage = new();

        /// <summary>
        /// Gets the <see cref="TimeTextInfo"/> for the English language.
        /// </summary>
        public static TimeTextInfo English => new(
            pluralRule: PluralRules.GetPluralRule("en"),
            week: new[] { "{0} week", "{0} weeks" },
            day: new[] { "{0} day", "{0} days" },
            hour: new[] { "{0} hour", "{0} hours" },
            minute: new[] { "{0} minute", "{0} minutes" },
            second: new[] { "{0} second", "{0} seconds" },
            millisecond: new[] { "{0} millisecond", "{0} milliseconds" },
            w: new[] { "{0}w" },
            d: new[] { "{0}d" },
            h: new[] { "{0}h" },
            m: new[] { "{0}m" },
            s: new[] { "{0}s" },
            ms: new[] { "{0}ms" },
            lessThan: "less than {0}"
        );

        /// <summary>
        /// Gets the <see cref="TimeTextInfo"/> for the French language.
        /// </summary>
        public static TimeTextInfo French => new(
            PluralRules.GetPluralRule("fr"),
            new[] { "{0} semaine", "{0} semaines" },
            new[] { "{0} jour", "{0} jours" },
            new[] { "{0} heure", "{0} heures" },
            new[] { "{0} minute", "{0} minutes" },
            new[] { "{0} seconde", "{0} secondes" },
            new[] { "{0} milliseconde", "{0} millisecondes" },
            new[] { "{0}sem" },
            new[] { "{0}j" },
            new[] { "{0}h" },
            new[] { "{0}m" },
            new[] { "{0}s" },
            new[] { "{0}ms" },
            "moins que {0}"
        );

        /// <summary>
        /// Gets the <see cref="TimeTextInfo"/> for the Spanish language.
        /// </summary>
        public static TimeTextInfo Spanish => new(
            PluralRules.GetPluralRule("es"),
            new[] { "{0} semana", "{0} semanas" },
            new[] { "{0} día", "{0} días" },
            new[] { "{0} hore", "{0} horas" },
            new[] { "{0} minuto", "{0} minutos" },
            new[] { "{0} segundo", "{0} segundos" },
            new[] { "{0} milisegundo", "{0} milisegundos" },
            new[] { "{0}sem" },
            new[] { "{0}d" },
            new[] { "{0}h" },
            new[] { "{0}m" },
            new[] { "{0}s" },
            new[] { "{0}ms" },
            "menos que {0}"
        );

        /// <summary>
        /// Gets the <see cref="TimeTextInfo"/> for the Portuguese language.
        /// </summary>
        public static TimeTextInfo Portuguese => new(
            PluralRules.GetPluralRule("pt"),
            new[] { "{0} semana", "{0} semanas" },
            new[] { "{0} dia", "{0} dias" },
            new[] { "{0} hora", "{0} horas" },
            new[] { "{0} minuto", "{0} minutos" },
            new[] { "{0} segundo", "{0} segundos" },
            new[] { "{0} milissegundo", "{0} milissegundos" },
            new[] { "{0}sem" },
            new[] { "{0}d" },
            new[] { "{0}h" },
            new[] { "{0}m" },
            new[] { "{0}s" },
            new[] { "{0}ms" },
            "menos do que {0}"
        );

        /// <summary>
        /// Gets the <see cref="TimeTextInfo"/> for the Italian language.
        /// </summary>
        public static TimeTextInfo Italian => new(
            PluralRules.GetPluralRule("it"),
            new[] { "{0} settimana", "{0} settimane" },
            new[] { "{0} giorno", "{0} giorni" },
            new[] { "{0} ora", "{0} ore" },
            new[] { "{0} minuto", "{0} minuti" },
            new[] { "{0} secondo", "{0} secondi" },
            new[] { "{0} millisecondo", "{0} millisecondi" },
            new[] { "{0}set" },
            new[] { "{0}g" },
            new[] { "{0}h" },
            new[] { "{0}m" },
            new[] { "{0}s" },
            new[] { "{0}ms" },
            "meno di {0}"
        );

        /// <summary>
        /// Gets the <see cref="TimeTextInfo"/> for the German language.
        /// </summary>
        public static TimeTextInfo German => new(
            PluralRules.GetPluralRule("de"),
            new[] { "{0} Woche", "{0} Wochen" },
            new[] { "{0} Tag", "{0} Tage" },
            new[] { "{0} Stunde", "{0} Stunden" },
            new[] { "{0} Minute", "{0} Minuten" },
            new[] { "{0} Sekunde", "{0} Sekunden" },
            new[] { "{0} Millisekunde", "{0} Millisekunden" },
            new[] { "{0}w" },
            new[] { "{0}t" },
            new[] { "{0}h" },
            new[] { "{0}m" },
            new[] { "{0}s" },
            new[] { "{0}ms" },
            "weniger als {0}"
        );

        /// <summary>
        /// Adds a <see cref="TimeTextInfo"/> for a language.
        /// </summary>
        /// <param name="twoLetterISOLanguageName">The string to get the associated <see cref="System.Globalization.CultureInfo"/></param>
        /// <param name="timeTextInfo">The localized <see cref="TimeTextInfo"/></param>
        public static void AddLanguage(string twoLetterISOLanguageName, TimeTextInfo timeTextInfo)
        {
            var c = twoLetterISOLanguageName.ToLower();
            _customLanguage.Add(c, timeTextInfo);
        }

        /// <summary>
        /// Gets the <see cref="TimeTextInfo"/> for a certain language.
        /// If the language is not implemented, the result will be <see langword="null"/>.
        /// </summary>
        /// <param name="twoLetterISOLanguageName"></param>
        /// <returns>
        /// The <see cref="TimeTextInfo"/> for a certain language.
        /// If the language is not implemented, the result will be <see langword="null"/>.
        /// </returns>
        /// <remarks>
        /// Custom languages can be added with <see cref="AddLanguage"/>.
        /// Custom languages override any built-in language with the same twoLetterISOLanguageName.
        /// </remarks>
        public static TimeTextInfo? GetTimeTextInfo(string twoLetterISOLanguageName)
        {
            if (_customLanguage.TryGetValue(twoLetterISOLanguageName, out var timeTextInfo))
                return timeTextInfo;

            return twoLetterISOLanguageName switch
            {
                "en" => English,
                "fr" => French,
                "es" => Spanish,
                "pt" => Portuguese,
                "it" => Italian,
                "de" => German,
                _ => null
            };
        }
    }
}