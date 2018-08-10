#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace SQLAzureMWUtils
{
    public class CommentAreaHelper
    {
        public List<CommentArea> CommentAreas = new List<CommentArea>();
        public bool CommentContinued = false;
        public bool CommentContinuedFromLastCommand = false;
        public int CommentNestedLevel = 0;
        public int CommentNestedLevelFromLastCommand = 0;
        public string[] Lines;
        public int CrLf = 2;

        public void FindCommentAreas(string sqlStr)
        {
            CommentAreas.Clear();

            CommentArea ca = null;
            if (sqlStr == null) return;

            CrLf = 2;

            try
            {
                Lines = Regex.Split(sqlStr, "\r\n");
                if (Lines.Count() == 1)
                {
                    Lines = Regex.Split(sqlStr, "\n");
                    CrLf = 1;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return;
            }

            bool bInComment = false;
            int nestedLevel = 0;
            int totalCharacterOffset = 0;

            if (CommentContinuedFromLastCommand)
            {
                bInComment = true;
                ca = new CommentArea();
                ca.Start = 0;
                nestedLevel = CommentNestedLevelFromLastCommand;
            }

            foreach (string line in Lines)
            {
                for (int idx = 0; idx < line.Length - 1; idx++)
                {
                    if (!bInComment && line[idx] == '-' && line[idx + 1] == '-')
                    {
                        if (idx == 0 || (idx > 0 && line[idx - 1] != '\''))
                        {
                            ca = new CommentArea();
                            ca.Start = totalCharacterOffset + idx;
                            ca.End = totalCharacterOffset + line.Length;
                            CommentAreas.Add(ca);
                            break;
                        }
                    }

                    if (line[idx] == '/' && line[idx + 1] == '*')
                    {
                        if (idx == 0 || (idx > 0 && line[idx - 1] != '\''))
                        {
                            if (bInComment)
                            {
                                nestedLevel++;
                            }
                            else
                            {
                                nestedLevel = 1;
                                bInComment = true;
                                ca = new CommentArea();
                                ca.Start = totalCharacterOffset + idx;
                            }
                            ++idx;
                            continue;
                        }
                    }

                    if (line[idx] == '*' && line[idx + 1] == '/')
                    {
                        if (bInComment)
                        {
                            nestedLevel--;
                            if (nestedLevel == 0)
                            {
                                ca.End = totalCharacterOffset + idx + 1;
                                CommentAreas.Add(ca);
                                bInComment = false;
                            }
                        }
                        ++idx;
                    }
                }

                totalCharacterOffset += line.Length + CrLf;
            }

            if (bInComment)
            {
                ca.End = totalCharacterOffset;
                CommentAreas.Add(ca);
            }
            CommentContinued = bInComment;
            CommentNestedLevel = nestedLevel;
        }

        public bool IsIndexInComments(long index)
        {
            if (CommentAreas == null || CommentAreas.Count < 1) return false;

            try
            {
                foreach (CommentArea ca in CommentAreas)
                {
                    if (ca.Start <= index && ca.End > index)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return false;
        }

        public bool IsMatchInComments(Match exp)
        {
            if (CommentAreas == null || CommentAreas.Count < 1) return false;

            try
            {
            // Ok, we found a match, but was it in a comment area
            foreach (CommentArea ca in CommentAreas)
            {
                if (exp.Index > ca.Start && exp.Index < ca.End)
                {
                    return true;
                }
            }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return false;
        }
    }
}
