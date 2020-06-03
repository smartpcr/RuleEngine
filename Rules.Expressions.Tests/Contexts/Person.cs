// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Person.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests.Contexts
{
    using System;
    using System.Collections.Generic;

    public class Person
    {
        public DateTime? BirthDate { get; set; }
        public string SSN { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender? Gender { get; set; }
        public Race? Race { get; set; }
        public int Age { get; set; }
        public Person Spouse { get; set; }
        public Person Dad { get; set; }
        public Person Mom { get; set; }
        public Person[] Children { get; set; }
        public List<string> Hobbies { get; set; }
        public string[] Titles { get; set; }
    }
    
    public enum Gender
    {
        Male,
        Female
    }

    public enum Race
    {
        White,
        Black,
        Asian,
        Hispanic,
        NativeIndian
    }
}