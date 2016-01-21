//---------------------------------------------------------------------------------------
// Copyright 2014 North Carolina State University
//
// Center for Educational Informatics
// http://www.cei.ncsu.edu/
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
//   * Redistributions of source code must retain the above copyright notice, this 
//     list of conditions and the following disclaimer.
//   * Redistributions in binary form must reproduce the above copyright notice, 
//     this list of conditions and the following disclaimer in the documentation 
//     and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
// OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//---------------------------------------------------------------------------------------
using System;
using System.Text;

namespace IntelliMedia
{
    public class Cipher
	{
        public static string Encrypt(string text)
        {
            string key = "q5eSd3Sc9FVphW5Lr8RxGYZn";
            StringBuilder result = new StringBuilder();

            for (int c = 0; c < text.Length; c++)
                result.Append((char)((uint)text[c] ^ (uint)key[c % key.Length]));

            byte[] bytesToEncode = System.Text.Encoding.UTF8.GetBytes(result.ToString());
            string encodedText = Convert.ToBase64String(bytesToEncode);

            return encodedText;
        }

        public static string Decrypt(string text)
        {
            string key = "q5eSd3Sc9FVphW5Lr8RxGYZn";

            byte[] decodedBytes = Convert.FromBase64String(text);
            string decodedText = System.Text.Encoding.UTF8.GetString(decodedBytes, 0, decodedBytes.Length);

            var result = new StringBuilder();

            for (int c = 0; c < decodedText.Length; c++)
                result.Append((char)((uint)decodedText[c] ^ (uint)key[c % key.Length]));

            return result.ToString();
        }
	}
}

