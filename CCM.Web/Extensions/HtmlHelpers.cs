/*
 * Copyright (c) 2017 Sveriges Radio AB, Stockholm, Sweden
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

﻿using System;
using System.Linq.Expressions;
using System.Web.Mvc;

public static class HtmlHelpers
{
    /// Code started from SO: 
    /// http://stackoverflow.com/questions/6578495/how-do-i-display-the-displayattribute-description-attribute-value
    /// <summary>
    /// Usage:
    /// @Html.DisplayFor(m => m.PropertyName)
    /// 
    /// supply cssclass name, and override span with div tag
    /// @Html.DisplayFor(m => m.PropertyName, "desc", "div")
    /// 
    /// using the named param
    /// @Html.DisplayFor(m => m.PropertyName, tagName: "div")
    /// </summary>
    public static MvcHtmlString DescriptionFor<TModel, TValue>(this HtmlHelper<TModel> self, Expression<Func<TModel, TValue>> expression, string cssClassName = "", string tagName = "span")
    {
        var metadata = ModelMetadata.FromLambdaExpression(expression, self.ViewData);
        var description = metadata.Description;

        if (!string.IsNullOrEmpty(description))
        {
            var tag = new TagBuilder(tagName) { InnerHtml = description };
            if (!string.IsNullOrEmpty(cssClassName))
            {
                tag.AddCssClass(cssClassName);
            }

            return new MvcHtmlString(tag.ToString());
        }

        return MvcHtmlString.Empty;
    }
}
