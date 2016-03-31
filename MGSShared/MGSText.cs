using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace MGSShared
{
    class MGSText
    {
        private static string[] Table;

        #region "Character Table"
        public static void Initialize()
        {
            Table = new string[0x10000];

            Table[0x1F01] = "Ä";
            Table[0x1F02] = "Å";
            Table[0x1F03] = "Ç";
            Table[0x1F04] = "É";
            Table[0x1F05] = "Ñ";
            Table[0x1F06] = "Ö";
            Table[0x1F07] = "Ü";
            Table[0x1F08] = "á";
            Table[0x1F09] = "à";
            Table[0x1F0A] = "â";
            Table[0x1F0B] = "ä";
            Table[0x1F0C] = "ã";
            Table[0x1F0D] = "å";
            Table[0x1F0E] = "ç";
            Table[0x1F0F] = "é";
            Table[0x1F10] = "è";
            Table[0x1F11] = "ê";
            Table[0x1F12] = "ë";
            Table[0x1F13] = "í";
            Table[0x1F14] = "ì";
            Table[0x1F15] = "î";
            Table[0x1F16] = "ï";
            Table[0x1F17] = "ñ";
            Table[0x1F18] = "ó";
            Table[0x1F19] = "ò";
            Table[0x1F1A] = "ô";
            Table[0x1F1B] = "ö";
            Table[0x1F1C] = "õ";
            Table[0x1F1D] = "ú";
            Table[0x1F1E] = "ù";
            Table[0x1F1F] = "û";
            Table[0x1F20] = "ü";
            Table[0x1F21] = "†";
            Table[0x1F22] = "°";
            Table[0x1F23] = "¢";
            Table[0x1F24] = "£";
            Table[0x1F25] = "§";
            Table[0x1F26] = "•";
            Table[0x1F27] = "¶";
            Table[0x1F28] = "ß";
            Table[0x1F29] = "®";
            Table[0x1F2A] = "©";
            Table[0x1F2B] = "™";
            Table[0x1F2C] = "´";
            Table[0x1F2D] = "¨";
            Table[0x1F2E] = "≠";
            Table[0x1F2F] = "Æ";
            Table[0x1F30] = "Ø";
            Table[0x1F31] = "∞";
            Table[0x1F32] = "±";
            Table[0x1F33] = "≤";
            Table[0x1F34] = "≥";
            Table[0x1F35] = "¥";
            Table[0x1F36] = "µ";
            Table[0x1F37] = "∂";
            Table[0x1F38] = "∑";
            Table[0x1F39] = "∏";
            Table[0x1F3A] = "π";
            Table[0x1F3B] = "∫";
            Table[0x1F3C] = "ª";
            Table[0x1F3D] = "º";
            Table[0x1F3E] = "Ω";
            Table[0x1F3F] = "æ";
            Table[0x1F40] = "ø";
            Table[0x1F41] = "¿";
            Table[0x1F42] = "¡";
            Table[0x1F43] = "¬";
            Table[0x1F44] = "√";
            Table[0x1F45] = "ƒ";
            Table[0x1F46] = "≈";
            Table[0x1F47] = "∆";
            Table[0x1F48] = "«";
            Table[0x1F49] = "»";
            Table[0x1F4A] = "…";
            Table[0x1F4B] = "n";
            Table[0x1F4C] = "À";
            Table[0x1F4D] = "Ã";
            Table[0x1F4E] = "Õ";
            Table[0x1F4F] = "Œ";
            Table[0x1F50] = "œ";
            Table[0x1F51] = "–";
            Table[0x1F52] = "—";
            Table[0x1F53] = "“";
            Table[0x1F54] = "”";
            Table[0x1F55] = "‘";
            Table[0x1F56] = "’";
            Table[0x1F57] = "÷";
            Table[0x1F58] = "◊";
            Table[0x1F59] = "ÿ";
            Table[0x1F5A] = "Ÿ";
            Table[0x1F5B] = "⁄";
            Table[0x1F5C] = "€";
            Table[0x1F5D] = "‹";
            Table[0x1F5E] = "›";
            Table[0x1F5F] = "ﬁ";
            Table[0x1F60] = "ﬂ";
            Table[0x1F61] = "‡";
            Table[0x1F62] = "·";
            Table[0x1F63] = "‚";
            Table[0x1F64] = "„";
            Table[0x1F65] = "‰";
            Table[0x1F66] = "Â";
            Table[0x1F67] = "Ê";
            Table[0x1F68] = "Á";
            Table[0x1F69] = "Ë";
            Table[0x1F6A] = "È";
            Table[0x1F6B] = "Í";
            Table[0x1F6C] = "Î";
            Table[0x1F6D] = "Ï";
            Table[0x1F6E] = "Ì";
            Table[0x1F6F] = "Ó";
            Table[0x1F70] = "Ô";
            Table[0x1F72] = "Ò";
            Table[0x1F73] = "Ú";
            Table[0x1F74] = "Û";
            Table[0x1F75] = "Ù";
            Table[0x1F76] = "ı";
            Table[0x1F77] = "ˆ";
            Table[0x1F78] = "˜";
            Table[0x1F79] = "¯";
            Table[0x1F7A] = "˘";
            Table[0x1F7B] = "˙";
            Table[0x1F7C] = "˚";
            Table[0x1F7D] = "¸";
            Table[0x1F7E] = "˝";
            Table[0x8101] = "ぁ";
            Table[0x8102] = "あ";
            Table[0x8103] = "ぃ";
            Table[0x8104] = "い";
            Table[0x8105] = "ぅ";
            Table[0x8106] = "う";
            Table[0x8107] = "ぇ";
            Table[0x8108] = "え";
            Table[0x8109] = "ぉ";
            Table[0x810A] = "お";
            Table[0x810B] = "か";
            Table[0x810C] = "が";
            Table[0x810D] = "き";
            Table[0x810E] = "ぎ";
            Table[0x810F] = "く";
            Table[0x8110] = "ぐ";
            Table[0x8111] = "け";
            Table[0x8112] = "げ";
            Table[0x8113] = "こ";
            Table[0x8114] = "ご";
            Table[0x8115] = "さ";
            Table[0x8116] = "ざ";
            Table[0x8117] = "し";
            Table[0x8118] = "じ";
            Table[0x8119] = "す";
            Table[0x811A] = "ず";
            Table[0x811B] = "せ";
            Table[0x811C] = "ぜ";
            Table[0x811D] = "そ";
            Table[0x811E] = "ぞ";
            Table[0x811F] = "た";
            Table[0x8120] = "だ";
            Table[0x8121] = "ち";
            Table[0x8122] = "ぢ";
            Table[0x8123] = "っ";
            Table[0x8124] = "つ";
            Table[0x8125] = "づ";
            Table[0x8126] = "て";
            Table[0x8127] = "で";
            Table[0x8128] = "と";
            Table[0x8129] = "ど";
            Table[0x812A] = "な";
            Table[0x812B] = "に";
            Table[0x812C] = "ぬ";
            Table[0x812D] = "ね";
            Table[0x812E] = "の";
            Table[0x812F] = "は";
            Table[0x8130] = "ば";
            Table[0x8131] = "ぱ";
            Table[0x8132] = "ひ";
            Table[0x8133] = "び";
            Table[0x8134] = "ぴ";
            Table[0x8135] = "ふ";
            Table[0x8136] = "ぶ";
            Table[0x8137] = "ぷ";
            Table[0x8138] = "へ";
            Table[0x8139] = "べ";
            Table[0x813A] = "ぺ";
            Table[0x813B] = "ほ";
            Table[0x813C] = "ぼ";
            Table[0x813D] = "ぽ";
            Table[0x813E] = "ま";
            Table[0x813F] = "み";
            Table[0x8140] = "む";
            Table[0x8141] = "め";
            Table[0x8142] = "も";
            Table[0x8143] = "ゃ";
            Table[0x8144] = "や";
            Table[0x8145] = "ゅ";
            Table[0x8146] = "ゆ";
            Table[0x8147] = "ょ";
            Table[0x8148] = "よ";
            Table[0x8149] = "ら";
            Table[0x814A] = "り";
            Table[0x814B] = "る";
            Table[0x814C] = "れ";
            Table[0x814D] = "ろ";
            Table[0x814E] = "ゎ";
            Table[0x814F] = "わ";
            Table[0x8150] = "ゐ";
            Table[0x8151] = "ん";
            Table[0x8152] = "ァ";
            Table[0x8153] = "ア";
            Table[0x8154] = "ィ";
            Table[0x8155] = "イ";
            Table[0x8156] = "ゥ";
            Table[0x8157] = "ウ";
            Table[0x8158] = "ェ";
            Table[0x8159] = "エ";
            Table[0x815A] = "ォ";
            Table[0x815B] = "オ";
            Table[0x815C] = "カ";
            Table[0x815D] = "ガ";
            Table[0x815E] = "キ";
            Table[0x815F] = "ギ";
            Table[0x8160] = "ク";
            Table[0x8161] = "グ";
            Table[0x8162] = "ケ";
            Table[0x8163] = "ゲ";
            Table[0x8164] = "コ";
            Table[0x8165] = "ゴ";
            Table[0x8166] = "サ";
            Table[0x8167] = "ザ";
            Table[0x8168] = "シ";
            Table[0x8169] = "ジ";
            Table[0x816A] = "ス";
            Table[0x816B] = "ズ";
            Table[0x816C] = "セ";
            Table[0x816D] = "ゼ";
            Table[0x816E] = "ソ";
            Table[0x816F] = "ゾ";
            Table[0x8170] = "タ";
            Table[0x8171] = "ダ";
            Table[0x8172] = "チ";
            Table[0x8173] = "ヂ";
            Table[0x8174] = "ッ";
            Table[0x8175] = "ツ";
            Table[0x8176] = "ヅ";
            Table[0x8177] = "テ";
            Table[0x8178] = "デ";
            Table[0x8179] = "ト";
            Table[0x817A] = "ド";
            Table[0x817B] = "ナ";
            Table[0x817C] = "ニ";
            Table[0x817D] = "ヌ";
            Table[0x817E] = "ネ";
            Table[0x817F] = "ノ";
            Table[0x8180] = "ハ";
            Table[0x8181] = "バ";
            Table[0x8182] = "パ";
            Table[0x8183] = "ヒ";
            Table[0x8184] = "ビ";
            Table[0x8185] = "ピ";
            Table[0x8186] = "フ";
            Table[0x8187] = "ブ";
            Table[0x8188] = "プ";
            Table[0x8189] = "ヘ";
            Table[0x818A] = "ベ";
            Table[0x818B] = "ペ";
            Table[0x818C] = "ホ";
            Table[0x818D] = "ボ";
            Table[0x818E] = "ポ";
            Table[0x818F] = "マ";
            Table[0x8190] = "ミ";
            Table[0x8191] = "ム";
            Table[0x8192] = "メ";
            Table[0x8193] = "モ";
            Table[0x8194] = "ャ";
            Table[0x8195] = "ヤ";
            Table[0x8196] = "ュ";
            Table[0x8197] = "ユ";
            Table[0x8198] = "ョ";
            Table[0x8199] = "ヨ";
            Table[0x819A] = "ラ";
            Table[0x819B] = "リ";
            Table[0x819C] = "ル";
            Table[0x819D] = "レ";
            Table[0x819E] = "ロ";
            Table[0x819F] = "ヮ";
            Table[0x81A0] = "ワ";
            Table[0x81A1] = "ヲ";
            Table[0x81A2] = "ン";
            Table[0x81A3] = "ヴ";
            Table[0x81A4] = "ヵ";
            Table[0x81A5] = "ヶ";
            Table[0xC307] = "！";
            Table[0xC308] = "？";
            Table[0xC309] = "。";
            Table[0xC30A] = "」";
            Table[0xC30B] = "』";
            Table[0xC30C] = "］";
            Table[0xC30D] = "〉";
            Table[0xC30E] = "》";
            Table[0xC30F] = "】";
            Table[0xC310] = "”";
            Table[0xC311] = "、";
            Table[0xC312] = "’";
            Table[0xC313] = "‐";
            Table[0xC314] = "―";
            Table[0xC315] = "…";
            Table[0xC316] = "‥";
            Table[0xC317] = "「";
            Table[0xC318] = "『";
            Table[0xC319] = "《";
            Table[0xC31A] = "［";
            Table[0xC31B] = "【";
            Table[0xC31C] = "“";
            Table[0xC31D] = "‘";
        }
        #endregion

        public static string Buffer2Text(byte[] Data, MGSGame Game)
        {
            switch (Game)
            {
                case MGSGame.MGS3:
                    StringBuilder Output = new StringBuilder();

                    for (int Index = 0; Index < Data.Length; Index++)
                    {
                        if (Data[Index] < 0x20 || Data[Index] > 0x7e)
                        {
                            if (Data[Index] == 0xa)
                                Output.Append(Environment.NewLine);
                            else if (Data[Index] == 0)
                                Output.Append("[end]");
                            else
                            {
                                int Value = Data[Index];
                                if (Index + 1 < Data.Length) Value = (Value << 8) | Data[++Index];

                                if (Table[Value] == null)
                                    Output.Append("\\x" + Value.ToString("X4"));
                                else
                                    Output.Append(Table[Value]);
                            }
                        }
                        else
                            Output.Append((char)Data[Index]);
                    }

                    return Output.ToString();

                case MGSGame.MGS4: return MGS2Text(Encoding.UTF8.GetString(Data));
            }

            return null;
        }

        private static string MGS2Text(string Text)
        {
            //Make text editable with notepad
            Text = Text.Replace(((char)0xa).ToString(), Environment.NewLine);
            Text = Text.Replace(((char)0).ToString(), "[end]");

            return Text;
        }

        public static byte[] Text2Buffer(string Text, MGSGame Game)
        {
            switch (Game)
            {
                case MGSGame.MGS3:
                    Text = Text2MGS(Text);

                    using (MemoryStream Output = new MemoryStream())
                    {
                        for (int Index = 0; Index < Text.Length; Index++)
                        {
                            if (Index < Text.Length - 1 && Text.Substring(Index, 2) == "\\x")
                            {
                                string Hex = Text.Substring(Index + 2, 4);
                                ushort Value = ushort.Parse(Hex, NumberStyles.HexNumber);
                                Output.WriteByte((byte)(Value >> 8));
                                Output.WriteByte((byte)Value);
                                Index += 5;
                            }
                            else
                            {
                                bool InRange = Text[Index] < 0x20 || Text[Index] > 0x7e;
                                if (InRange && Text[Index] != 0 && Text[Index] != 0xa)
                                {
                                    int Value = Array.IndexOf(Table, Text.Substring(Index, 1));

                                    if (Value > -1)
                                    {
                                        Output.WriteByte((byte)(Value >> 8));
                                        Output.WriteByte((byte)Value);
                                    }
                                }
                                else
                                    Output.WriteByte((byte)Text[Index]);
                            }
                        }

                        return Output.ToArray();
                    }

                case MGSGame.MGS4: return Encoding.UTF8.GetBytes(Text2MGS(Text));
            }

            return null;
        }

        private static string Text2MGS(string Text)
        {
            //Undo the changes made by MGS2Text
            Text = Text.Replace(Environment.NewLine, ((char)0xa).ToString());
            Text = Text.Replace("[end]", ((char)0).ToString());

            return Text;
        }
    }
}
