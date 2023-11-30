using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GymSimulation
{
    public static class Util
    {
        #region Convert
        /// <summary>
        /// 1ByteデータのBitの状態の取得
        ///  7 6 5 4 3 2 1 0の並び
        /// </summary>
        /// <param name="data">1Byteデータ</param>
        /// <param name="bit">0～7を指定</param>
        /// <returns>true: bit=1 false: bit=0</returns>
        public static bool ToBit(byte data, byte bit)
        {
            if (((data >> bit) & 0x01) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Enumデータに設定された数値を取得
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int Enum2Val(Enum data)
        {
            return Convert.ToInt32(data.ToString("D"));
        }

        /// <summary>
        /// 数値から該当するEnumデータを取得。該当しない場合はnullを返す
        /// </summary>
        /// <param name="value"></param>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static object Val2Enum(object value, Type enumType)
        {
            object ret = null;
            if (Enum.IsDefined(enumType, value))
            {
                ret = Enum.Parse(enumType, value.ToString());
            }
            return ret;
        }

        /// <summary>
        /// 数値から該当するEnumデータが存在するかチェック
        /// </summary>
        /// <param name="value"></param>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static bool IsEnumFound(object value, Type enumType)
        {
            return Enum.IsDefined(enumType, value);
        }

        /// <summary>
        /// 入力文字列が10進数で8Byte整数に変換できるかチェック
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNumeric(string value)
        {
            foreach (var c in value)
            {
                if (c < '0' || '9' < c) return false;
            }

            return true;
        }

        /// <summary>
        /// 入力文字列が16進数で8Byte整数に変換できるかチェック
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsHexadecimal(string value)
        {
            foreach (var c in value)
            {
                if (('0' <= c && c <= '9') || ('a' <= c && c <= 'f') || ('A' <= c && c <= 'F')) continue;

                return false;
            }

            return true;
        }

        /// <summary>
        /// 文字列データを大文字にし、桁揃えする
        ///      16進数: 桁数に合わせてゼロパディングする
        ///      例) ("F", 2) -> "0F" / ("F", 4) -> "000F"
        ///          ("1fx", 2) -> "1FX"
        /// </summary>
        /// <param name="data">データ</param>
        /// <param name="padLength">桁数</param>
        /// <returns></returns>
        public static string FmtStr(string data, int padLength)
        {
            if (string.IsNullOrEmpty(data)) data = string.Empty;

            if (data.Length < padLength)
            {
                return data.ToUpper().PadLeft(padLength, '0');
            }
            else
            {
                return data.ToUpper();
            }
        }

        /// <summary>
        /// byteリストを16進数文字列に変換する(ビッグエンディアン固定)
        ///      例) 0x01, 0x02 -> "0102"
        /// </summary>
        /// <param name="data">byteリストデータ</param>
        /// <returns>変換後文字列</returns>
        public static string ToString(List<byte> data)
        {
            string ret = string.Empty;
            foreach (byte b in data)
            {
                ret += b.ToString("X2");
            }
            return ret;
        }

        /// <summary>
        /// byteリストを16進数文字列に変換する(ビッグエンディアン固定)
        /// 開始インデックス、変換リスト長指定
        /// 例) 0x01, 0x02, 0x03, 0x04  (data, 1, 3) => "020304"
        /// </summary>
        /// <param name="data">byteリストデータ</param>
        /// <param name="startIndex">開始インデックス</param>
        /// <param name="length">変換リスト長</param>
        /// <returns>変換後文字列</returns>
        //public static string ToString(List<byte> data, int startIndex, int length)
        //{
            //return ToString(data, startIndex, length, Endian.Big);
        //}

        /// <summary>
        /// byteリストを16進数文字列に変換する
        /// 開始インデックス、変換リスト長、エンディアン指定
        /// 例) 0x01, 0x02, 0x03, 0x04  (data, 1, 3, true) => "040302"
        /// </summary>
        /// <param name="data">byteリストデータ</param>
        /// <param name="startIndex">開始インデックス</param>
        /// <param name="length">変換リスト長</param>
        /// <param name="isLittleEndian">true:リトルエンディアン false:ビッグエンディアン</param>
        /// <returns>変換後文字列</returns>
        public static string ToString(List<byte> data, int startIndex, int length, bool isLittleEndian)
        {
            if (startIndex < 0 || data.Count <= startIndex) startIndex = 0;
            if (startIndex + length > data.Count) length = data.Count - startIndex;

            string ret = string.Empty;

            for (int i = 0; i < length; i++)
            {
                if (!isLittleEndian)
                    ret = ret + data[i + startIndex].ToString("X2");
                else
                    ret = data[i + startIndex].ToString("X2") + ret;
            }

            return ret;
        }

        ///// <summary>
        ///// byte配列を16進数文字列に変換する(ビッグエンディアン固定)
        /////      例) 0x01, 0x02 -> "0102"
        ///// </summary>
        ///// <param name="data"></param>
        ///// <returns></returns>
        //public static string ToString(byte[] data)
        //{
        //    string ret = string.Empty;
        //    foreach (byte b in data)
        //    {
        //        ret += b.ToString("X2");
        //    }
        //    return ret;
        //}

        ///// <summary>
        ///// byte配列を16進数文字列に変換する(ビッグエンディアン固定)
        ///// 開始インデックス、変換リスト長指定
        ///// 例) 0x01, 0x02, 0x03, 0x04  (data, 1, 3) => "020304"
        ///// </summary>
        ///// <param name="data">byteリストデータ</param>
        ///// <param name="startIndex">開始インデックス</param>
        ///// <param name="length">変換リスト長</param>
        ///// <returns>変換後文字列</returns>
        //public static string ToString(byte[] data, int startIndex, int length)
        //{
        //    return ToString(data, startIndex, length, Endian.Big);
        //}

        /// <summary>
        /// byte配列を16進数文字列に変換する
        /// 開始インデックス、変換リスト長、エンディアン指定
        /// 例) 0x01, 0x02, 0x03, 0x04  (data, 1, 3, true) => "040302"
        /// </summary>
        /// <param name="data">byteリストデータ</param>
        /// <param name="startIndex">開始インデックス</param>
        /// <param name="length">変換リスト長</param>
        /// <param name="isLittleEndian">true:リトルエンディアン false:ビッグエンディアン</param>
        /// <returns>変換後文字列</returns>
        public static string ToString(byte[] data, int startIndex, int length, bool isLittleEndian)
        {
            if (startIndex < 0 || data.Length <= startIndex) startIndex = 0;
            if (startIndex + length > data.Length) length = data.Length - startIndex;

            string ret = string.Empty;

            for (int i = 0; i < length; i++)
            {
                if (!isLittleEndian)
                    ret = ret + data[i + startIndex].ToString("X2");
                else
                    ret = data[i + startIndex].ToString("X2") + ret;
            }

            return ret;
        }

        /// <summary>
        /// byteデータを16進数文字列に変換する
        /// 　　例) 0x01 -> "01"
        /// </summary>
        /// <param name="data">byteデータ</param>
        /// <returns>変換後文字列</returns>
        public static string ToString(byte data)
        {
            return data.ToString("X2");
        }

        /// <summary>
        /// byteデータを文字列に変換する
        ///    fromBase:16     16進数文字列に変換する
        ///    fromBase:16以外 10進数文字列に変換する
        /// </summary>
        /// <param name="data">byteデータ</param>
        /// <param name="fromBase">10:10進数文字列 16:16進数文字列</param>
        /// <returns>変換後文字列</returns>
        public static string ToString(byte data, int fromBase)
        {
            if (fromBase == 16)
            {
                return data.ToString("X2");
            }

            return data.ToString();
        }

        /// <summary>
        /// 32ビット符号有り整数からByteリストを生成する(BigEndian)
        ///   例) 1234 -> 0x04, 0xD2
        /// </summary>
        /// <param name="value">32ビット符号有り整数</param>
        /// <param name="byteSize">変換バイトサイズ</param>
        /// <returns>変換後Byteリスト</returns>
        public static List<byte> GetBytes(int value, int byteSize)
        {
            var ret = new List<byte>();
            for (int i = (byteSize - 1); i >= 0; i--)
            {
                ret.Add((byte)((value >> (i * 8)) & 0xFF));
            }
            return ret;
        }

        /// <summary>
        /// 16ビット符号無し整数から2Byteリストを生成する(BigEndian / LittleEndian選択可能)
        /// </summary>
        /// <param name="value">16ビット符号無し整数</param>
        /// <param name="isLittleEndian">true or 指定無し:Little / false:Big</param>
        /// <returns></returns>
        public static List<byte> GetBytes(UInt16 value, params bool[] isLittleEndian)
        {
            bool little = (isLittleEndian.Length != 0 && isLittleEndian[0] != true) ? false : true;

            byte[] ret = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian != little)
            {
                Array.Reverse(ret);
            }
            return ret.ToList<byte>();
        }

        /// <summary>
        /// 32ビット符号無し整数から4Byteリストを生成する(BigEndian / LittleEndian選択可能)
        /// </summary>
        /// <param name="value">32ビット符号無し整数</param>
        /// <param name="isLittleEndian">true or 指定無し:Little / false:Big</param>
        /// <returns></returns>
        public static List<byte> GetBytes(UInt32 value, params bool[] isLittleEndian)
        {
            bool little = (isLittleEndian.Length != 0 && isLittleEndian[0] != true) ? false : true;

            byte[] ret = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian != little)
            {
                Array.Reverse(ret);
            }
            return ret.ToList<byte>();
        }

        /// <summary>
        /// 32ビット符号無し整数から4Byteリストを生成する(BigEndian / LittleEndian選択可能)
        /// </summary>
        /// <param name="value">32ビット符号無し整数</param>
        /// <param name="isLittleEndian">true or 指定無し:Little / false:Big</param>
        /// <returns></returns>
        public static List<byte> GetBytes(UInt64 value, params bool[] isLittleEndian)
        {
            bool little = (isLittleEndian.Length != 0 && isLittleEndian[0] != true) ? false : true;

            byte[] ret = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian != little)
            {
                Array.Reverse(ret);
            }
            return ret.ToList<byte>();
        }

        /// <summary>
        /// 64ビット符号無し整数からByteリストを生成する(BigEndian)
        ///    変換サイズが8Byte以下の場合、データが欠落する恐れがある
        ///    例) 1122334455667788 -> 0x03 0xFC 0xC1 0xDA 0x8C 0x9C 0x4C
        /// </summary>
        /// <param name="value">64ビット符号無し整数</param>
        /// <param name="byteSize">変換バイトサイズ</param>
        /// <returns>変換後Byteリスト</returns>
        public static List<byte> GetBytes(UInt64 value, int byteSize)
        {
            var ret = new List<byte>();
            for (int i = (byteSize - 1); i >= 0; i--)
            {
                ret.Add((byte)((value >> (i * 8)) & 0xFF));
            }
            return ret;
        }

        /// <summary>
        /// 16進数文字列からByteリストを生成する(BigEndian)
        ///      "1234" -> 0x12, 0x34
        /// </summary>
        /// <param name="value">16進数文字列</param>
        /// <returns>変換後Byteリスト</returns>
        public static List<byte> GetBytes(string value)
        {
            if (!IsHexadecimal(value)) return null;
            if (value.Length % 2 != 0) value = "0" + value; //桁が足りないとき、"0"を付加

            var ret = new List<byte>();
            for (int i = 0; i < value.Length; i += 2)
            {
                ret.Add(Convert.ToByte(value.Substring(i, 2), 16));
            }
            return ret;
        }

        /// <summary>
        /// 16進数文字列からByteリストを生成する(BigEndian)
        ///     "1234" -> 0x12, 0x34
        /// </summary>
        /// <param name="value">16進数文字列</param>
        /// <param name="byteSize">変換バイトサイズ</param>
        /// <returns>変換後Byteリスト</returns>
        public static List<byte> GetBytes(string value, int byteSize)
        {
            if (!IsHexadecimal(value)) return null;

            string data = FmtStr(value, byteSize * 2);
            var ret = new List<byte>();

            for (int i = 0; i < byteSize * 2; i += 2)
            {
                ret.Add(Convert.ToByte(data.Substring(i, 2), 16));
            }
            return ret;
        }

        /// <summary>
        /// 文字列からByteリストを生成する(BigEndian)
        ///   fromBase=10: 入力文字列を10進文字列とする "1234" -> 0x04, 0xD2
        ///                ※64ビット符号無し整数を指定(最大値 "18446744073709551615")
        ///                  最大値を超える場合はnullを返す
        ///   fromBase=16: 入力文字列を16進文字列とする "1234" -> 0x12, 0x34
        /// </summary>
        /// <param name="value">入力文字列</param>
        /// <param name="byteSize">変換バイトサイズ</param>
        /// <param name="fromBase">10:10進数文字列 16:16進数文字列</param>
        /// <returns>変換後Byteリスト</returns>
        public static List<byte> GetBytes(string value, int byteSize, int fromBase)
        {
            if (fromBase == 16)
            {
                return GetBytes(value, byteSize);
            }

            if (fromBase == 10)
            {
                if (!IsNumeric(value)) return null;

                UInt64 data = 0;

                if (!UInt64.TryParse(value, out data)) return null;

                data = Convert.ToUInt64(value);

                return GetBytes(data, byteSize);
            }

            return null;
        }

        /// <summary>
        /// 16進数文字列からByte配列を生成する(BigEndian)
        ///      "1234" -> 0x12, 0x34
        /// </summary>
        /// <param name="value">入力文字列</param>
        /// <returns></returns>
        public static byte[] ToByteArray(string value)
        {
            var ret = new byte[(value.Length + 1) / 2];
            for (int i = 0; i < ret.Length; i++)
            {
                if ('0' <= value[i * 2] && value[i * 2] <= '9') ret[i] = (byte)((value[i * 2] - '0') << 4);
                else if ('a' <= value[i * 2] && value[i * 2] <= 'f') ret[i] = (byte)((0x0A + value[i * 2] - 'a') << 4);
                else if ('A' <= value[i * 2] && value[i * 2] <= 'F') ret[i] = (byte)((0x0A + value[i * 2] - 'A') << 4);
                else ret[i] = 0;

                if (i * 2 + 1 >= value.Length)
                {
                }
                else if ('0' <= value[i * 2 + 1] && value[i * 2 + 1] <= '9') ret[i] += (byte)((value[i * 2 + 1] - '0') & 0x0F);
                else if ('a' <= value[i * 2 + 1] && value[i * 2 + 1] <= 'f') ret[i] += (byte)((0x0A + value[i * 2 + 1] - 'a') & 0x0F);
                else if ('A' <= value[i * 2 + 1] && value[i * 2 + 1] <= 'F') ret[i] += (byte)((0x0A + value[i * 2 + 1] - 'A') & 0x0F);
            }
            return ret;
        }

        /// <summary>
        /// 16進数文字列を変換しByte配列にセットする(BigEndian)
        ///      "1234" -> 0x12, 0x34
        /// </summary>
        /// <param name="value">入力文字列</param>
        /// <param name="dstArray">セット先Byte配列</param>
        /// <param name="dstStartIndex">セット先Byte配列Index</param>
        public static void SetByteArray(string value, byte[] dstArray, int dstStartIndex)
        {
            int length = (value.Length + 1) / 2;
            for (int i = 0; i < length; i++)
            {
                if ('0' <= value[i * 2] && value[i * 2] <= '9') dstArray[dstStartIndex + i] = (byte)((value[i * 2] - '0') << 4);
                else if ('a' <= value[i * 2] && value[i * 2] <= 'f') dstArray[dstStartIndex + i] = (byte)((0x0A + value[i * 2] - 'a') << 4);
                else if ('A' <= value[i * 2] && value[i * 2] <= 'F') dstArray[dstStartIndex + i] = (byte)((0x0A + value[i * 2] - 'A') << 4);
                else dstArray[dstStartIndex + i] = 0;

                if (i * 2 + 1 >= value.Length)
                {
                }
                else if ('0' <= value[i * 2 + 1] && value[i * 2 + 1] <= '9') dstArray[dstStartIndex + i] += (byte)((value[i * 2 + 1] - '0') & 0x0F);
                else if ('a' <= value[i * 2 + 1] && value[i * 2 + 1] <= 'f') dstArray[dstStartIndex + i] += (byte)((0x0A + value[i * 2 + 1] - 'a') & 0x0F);
                else if ('A' <= value[i * 2 + 1] && value[i * 2 + 1] <= 'F') dstArray[dstStartIndex + i] += (byte)((0x0A + value[i * 2 + 1] - 'A') & 0x0F);
            }
        }
        /// <summary>
        /// 16進数文字列のエンディアン変換
        /// </summary>
        /// <param name="value">変換対象データ</param>
        /// <returns>変換後データ</returns>
        public static string SwapEndian(string value)
        {
            string ret = string.Empty;
            if (value.Length % 2 != 0) value = "0" + value; //桁が足りないとき、"0"を付加

            for (int i = value.Length - 2; i >= 0; i -= 2)
            {
                ret += value.Substring(i, 2);
            }

            return ret;
        }

        /// <summary>
        /// byte配列のエンディアン変換
        /// </summary>
        /// <param name="value">変換対象データ</param>
        /// <returns>変換後データ</returns>
        public static byte[] SwapEndian(byte[] value)
        {
            byte[] ret = new byte[value.Length];

            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = value[value.Length - 1 - i];
            }

            return ret;
        }

        /// <summary>
        /// byte Listのエンディアン変換
        /// </summary>
        /// <param name="value">変換対象データ</param>
        /// <returns>変換後データ</returns>
        public static List<byte> SwapEndian(List<byte> value)
        {
            List<byte> ret = new List<byte>();

            for (int i = 0; i < value.Count; i++)
            {
                ret.Add(value[value.Count - 1 - i]);
            }

            return ret;
        }

        /// <summary>
        /// 作業用ディレクトリを基準とした絶対パスを取得する
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFullPath(string path)
        {
            if (System.IO.Path.IsPathRooted(path))
                return path;
            else
                return System.IO.Path.GetFullPath(System.Environment.CurrentDirectory + @"\" + path);
        }

        /// <summary>
        /// 作業用ディレクトリを基準とした相対パスを取得する
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        //public static string GetRelativePath(string path)
        //{
            //Uri u1 = new Uri(System.Environment.CurrentDirectory + @"\");
            //Uri u2 = new Uri(path);

            //string ret = u1.MakeRelativeUri(u2).ToString();
            //ret = ret.Replace("+", "%2B");
            //return System.Web.HttpUtility.UrlDecode(ret).Replace("/", @"\");
        //}

        /// <summary>
        /// ファイル選択ダイアログを表示して指定フォルダを選択させる
        /// (相対パスに未対応)
        /// </summary>
        /// <param name="description">説明文</param>
        /// <param name="folderPath">フォルダパス</param>
        public static DialogResult SelectFolder(string description, ref string folderPath)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = description;
                dlg.FileName = "フォルダ選択";
                dlg.Filter = "フォルダ|.";
                dlg.ValidateNames = false;
                dlg.CheckFileExists = false;
                dlg.CheckPathExists = false;
                dlg.InitialDirectory = (Directory.Exists(folderPath) ? folderPath : Application.StartupPath);

                if (dlg.ShowDialog() == DialogResult.Cancel) return DialogResult.Cancel;

                folderPath = Path.GetDirectoryName(dlg.FileName);
            }

            return DialogResult.OK;
        }

        /// <summary>
        /// ファイル選択ダイアログを表示して指定ファイルを選択させる
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="filter">フィルタ</param>
        /// <param name="initialDirectory">初期フォルダ</param>
        /// <param name="filePath">ファイルパス</param>
        /// <returns></returns>
        public static DialogResult SelectFile(string title, string filter, string initialDirectory, ref string filePath)
        {
            if (string.IsNullOrEmpty(filter)) filter = "All|*.*";

            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = title;
                dlg.Filter = filter;
                if (File.Exists(filePath))
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(filePath);
                    dlg.FileName = Path.GetFileName(filePath);
                }
                else
                {
                    dlg.InitialDirectory = initialDirectory;
                    dlg.FileName = Path.GetFileName(filePath);
                }
                if (dlg.ShowDialog() == DialogResult.Cancel)
                {
                    return DialogResult.Cancel;
                }

                filePath = dlg.FileName;
            }

            return DialogResult.OK;
        }

        /// <summary>
        /// 名前を付けて保存ダイアログを表示してファイルを保存するパスを取得
        /// </summary>
        /// <param name="title">ダイアログのタイトル</param>
        /// <param name="filter">フィルター文字列 (例 "csv file|*.csv|All file|*.*") 空文字指定時は"All|*.*"になる</param>
        /// <param name="initialDirectory">初期ディレクトリ</param>
        /// <param name="filePath">ファイルパス</param>
        /// <returns></returns>
        public static DialogResult SaveFile(string title, string filter, string initialDirectory, ref string filePath)
        {
            if (string.IsNullOrEmpty(filter)) filter = "All|*.*";

            using (var dlg = new SaveFileDialog())
            {
                dlg.Title = title;
                dlg.Filter = filter;
                if (File.Exists(filePath))
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(filePath);
                }
                else
                {
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        dlg.InitialDirectory = Path.GetDirectoryName(filePath);
                        if (!Directory.Exists(dlg.InitialDirectory))
                        {
                            dlg.InitialDirectory = initialDirectory;
                        }
                    }
                    else
                    {
                        dlg.InitialDirectory = initialDirectory;
                    }
                }
                dlg.FileName = Path.GetFileName(filePath);
                if (dlg.ShowDialog() == DialogResult.Cancel)
                {
                    return DialogResult.Cancel;
                }

                filePath = dlg.FileName;
            }

            return DialogResult.OK;
        }

        private static readonly char[] hexCharTable = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        /// <summary>
        /// byte配列を16進数文字列に変換する(ビッグエンディアン固定)(unsafe高速)
        ///      例) 0x01, 0x02 -> "0102"
        /// </summary>
        /// <param name="bytes">byte配列</param>
        /// <returns>hexString</returns>
        public static string ToString(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return string.Empty;

            var destString = new string('0', bytes.Length << 1);

            unsafe
            {
                fixed (char* fixedDest = destString)
                fixed (byte* fixedBytes = &bytes[0])
                fixed (char* fixedTable = &hexCharTable[0])
                {
                    int* pdest = (int*)fixedDest;
                    byte* pbytes = fixedBytes;
                    char* ptable = fixedTable;

                    for (int i = 0; i < bytes.Length; i++)
                    {
                        *pdest = ptable[*pbytes >> 4];
                        *pdest += ptable[*pbytes & 0x0F] << 16;
                        pdest++;
                        pbytes++;
                    }
                }
            }

            return destString;
        }

        /// <summary>
        /// byte配列を16進数文字列に変換する(ビッグエンディアン固定)(unsafe高速)
        /// 開始インデックス、変換リスト長指定
        /// 例) 0x01, 0x02, 0x03, 0x04  (data, 1, 3) => "020304"
        /// </summary>
        /// <param name="bytes">byte配列</param>
        /// <param name="start">開始インデックス</param>
        /// <param name="length">変換リスト長</param>
        /// <returns>変換後文字列</returns>
        public static string ToString(byte[] bytes, int start, int length)
        {
            if (bytes == null || bytes.Length == 0) return string.Empty;

            if (start > bytes.Length) start = 0;
            if (start + length > bytes.Length) length = bytes.Length - start;

            var destString = new string('0', length << 1);

            unsafe
            {
                fixed (char* fixedDest = destString)
                fixed (byte* fixedBytes = &bytes[start])
                fixed (char* fixedTable = &hexCharTable[0])
                {
                    int* pdest = (int*)fixedDest;
                    byte* pbytes = fixedBytes;
                    char* ptable = fixedTable;

                    while (length > 0)
                    {
                        *pdest = ptable[*pbytes >> 4];
                        *pdest += ptable[*pbytes & 0x0F] << 16;
                        pdest++;
                        pbytes++;
                        length--;
                    }
                }
            }

            return destString;
        }
        #endregion
    }
}
