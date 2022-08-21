using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using HiddenUnits;

public class LanguageReader : MonoBehaviour
{
    public void Start()
    {
        var locField = typeof(Localizer).GetField("m_localization", BindingFlags.Static | BindingFlags.NonPublic);
        var languages = (Dictionary<Localizer.Language, Dictionary<string, string>>)locField.GetValue(null);
        try
        {
            for (int i = 0; i < localizerEN.key.Count; i++)
            {
                languages[Localizer.Language.LANG_EN_US].Add(localizerEN.key[i], localizerEN.value[i]);
            }
            for (int i = 0; i < localizerRU.key.Count; i++)
            {
                languages[Localizer.Language.LANG_RU].Add(localizerRU.key[i], localizerRU.value[i]);
            }
            for (int i = 0; i < localizerCH.key.Count; i++)
            {
                languages[Localizer.Language.LANG_CH].Add(localizerCH.key[i], localizerCH.value[i]);
            }
            for (int i = 0; i < localizerFR.key.Count; i++)
            {
                languages[Localizer.Language.LANG_FR].Add(localizerFR.key[i], localizerFR.value[i]);
            }
            for (int i = 0; i < localizerES.key.Count; i++)
            {
                languages[Localizer.Language.LANG_ES].Add(localizerES.key[i], localizerES.value[i]);
            }
            for (int i = 0; i < localizerJA.key.Count; i++)
            {
                languages[Localizer.Language.LANG_JA].Add(localizerJA.key[i], localizerJA.value[i]);
            }
            for (int i = 0; i < localizerDE.key.Count; i++)
            {
                languages[Localizer.Language.LANG_DE].Add(localizerDE.key[i], localizerDE.value[i]);
            }
            for (int i = 0; i < localizerIT.key.Count; i++)
            {
                languages[Localizer.Language.LANG_IT].Add(localizerIT.key[i], localizerIT.value[i]);
            }
            for (int i = 0; i < localizerPT_BR.key.Count; i++)
            {
                languages[Localizer.Language.LANG_PT_BR].Add(localizerPT_BR.key[i], localizerPT_BR.value[i]);
            }
        }
        catch
        {

        }
    }

    public AutoLocalizer localizerEN;

    public AutoLocalizer localizerRU;

    public AutoLocalizer localizerCH;

    public AutoLocalizer localizerFR;

    public AutoLocalizer localizerES;

    public AutoLocalizer localizerJA;

    public AutoLocalizer localizerDE;

    public AutoLocalizer localizerIT;

    public AutoLocalizer localizerPT_BR;
}