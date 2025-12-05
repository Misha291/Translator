using System;
using System.Collections.Generic;

namespace Assembler
{
    public class HackTranslator
    {
        private static int _nextVarAddress = 16;

        private string ConvertToAInstruction(int address)
        {
            return "0" + Convert.ToString(address, 2).PadLeft(15, '0');
        }

        public string[] TranslateAsmToHack(string[] instructions, Dictionary<string, int> symbolTable)
        {
            var result = new List<string>();
            _nextVarAddress = 16; // Сброс счетчика при новом переводе

            foreach (string instr in instructions)
            {
                if (instr.StartsWith("@"))
                {
                    result.Add(AInstructionToCode(instr, symbolTable));
                }
                else
                {
                    result.Add(CInstructionToCode(instr));
                }
            }

            return result.ToArray();
        }

        public string AInstructionToCode(string aInstruction, Dictionary<string, int> symbolTable)
        {
            string operand = aInstruction.Substring(1);
            int address;

            if (int.TryParse(operand, out address))
            {
                // Числовой операнд - оставляем как есть
            }
            else if (symbolTable.TryGetValue(operand, out address))
            {
                // Символ уже есть в таблице
            }
            else
            {
                // Новый символ - добавляем как переменную, начиная с адреса 16
                address = _nextVarAddress;
                symbolTable[operand] = address;
                _nextVarAddress++;
            }

            return ConvertToAInstruction(address);
        }

        public string CInstructionToCode(string cInstruction)
        {
            var compMap = new Dictionary<string, string>
            {
                {"0", "101010"}, {"1", "111111"}, {"-1", "111010"},
                {"D", "001100"}, {"A", "110000"}, {"M", "110000"},
                {"!D", "001101"}, {"!A", "110001"}, {"!M", "110001"},
                {"-D", "001111"}, {"-A", "110011"}, {"-M", "110011"},
                {"D+1", "011111"}, {"A+1", "110111"}, {"M+1", "110111"},
                {"D-1", "001110"}, {"A-1", "110010"}, {"M-1", "110010"},
                {"D+A", "000010"}, {"D+M", "000010"},
                {"D-A", "010011"}, {"D-M", "010011"},
                {"A-D", "000111"}, {"M-D", "000111"},
                {"D&A", "000000"}, {"D&M", "000000"},
                {"D|A", "010101"}, {"D|M", "010101"}
            };

            var destMap = new Dictionary<string, string>
            {
                {"", "000"}, {"M", "001"}, {"D", "010"}, {"MD", "011"},
                {"A", "100"}, {"AM", "101"}, {"AD", "110"}, {"AMD", "111"}
            };

            var jumpMap = new Dictionary<string, string>
            {
                {"", "000"}, {"JGT", "001"}, {"JEQ", "010"}, {"JGE", "011"},
                {"JLT", "100"}, {"JNE", "101"}, {"JLE", "110"}, {"JMP", "111"}
            };

            string jumpPart = "";
            string compPart;
            string destPart = "";

            // Разбираем инструкцию на части
            int semicolonIndex = cInstruction.IndexOf(';');
            if (semicolonIndex != -1)
            {
                jumpPart = cInstruction.Substring(semicolonIndex + 1);
                cInstruction = cInstruction.Substring(0, semicolonIndex);
            }

            int equalsIndex = cInstruction.IndexOf('=');
            if (equalsIndex != -1)
            {
                destPart = cInstruction.Substring(0, equalsIndex);
                compPart = cInstruction.Substring(equalsIndex + 1);
            }
            else
            {
                compPart = cInstruction;
            }

            // Получаем коды для компонентов
            if (!compMap.TryGetValue(compPart, out string compCode))
            {
                throw new FormatException($"Invalid comp part: {compPart}");
            }

            if (!destMap.TryGetValue(destPart, out string destCode))
            {
                throw new FormatException($"Invalid dest part: {destPart}");
            }

            if (!jumpMap.TryGetValue(jumpPart, out string jumpCode))
            {
                throw new FormatException($"Invalid jump part: {jumpPart}");
            }

            char aBit = compPart.Contains('M') ? '1' : '0';
            return $"111{aBit}{compCode}{destCode}{jumpCode}";
        }
    }
}
