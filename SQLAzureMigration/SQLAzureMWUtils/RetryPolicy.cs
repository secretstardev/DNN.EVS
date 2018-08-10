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

namespace SQLAzureMWUtils
{
    public class RetryPolicy
    {
        public int RetryCount { get; set; }
        public TimeSpan MinimunDelay { get; set; }
        public TimeSpan MaximunDelay { get; set; }
        public TimeSpan RetryIncrementalDelay { get; set; }

        private RetryPolicy()
        {
        }

        public RetryPolicy(int retryCount, TimeSpan minimunDelay, TimeSpan maximunDelay, TimeSpan incrementalDelay)
        {
            RetryCount = retryCount;
            MinimunDelay = minimunDelay;
            MaximunDelay = maximunDelay;
            RetryIncrementalDelay = incrementalDelay;
        }

        public bool ShouldRetry(int retryCount, Exception lastException, out TimeSpan delay)
        {
            if (retryCount < RetryCount)
            {
                var random = new Random();

                var delta = (int)((Math.Pow(2.0, retryCount) - 1.0) * random.Next((int)(RetryIncrementalDelay.TotalMilliseconds * 0.8), (int)(RetryIncrementalDelay.TotalMilliseconds * 1.2)));
                var interval = (int) Math.Min(checked(MinimunDelay.TotalMilliseconds + delta), MaximunDelay.TotalMilliseconds);

                delay = TimeSpan.FromMilliseconds(interval);

                return true;
            }

            delay = TimeSpan.Zero;
            return false;
        }
    }
}