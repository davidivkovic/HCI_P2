﻿using System;
using P2.Primitives;

namespace P2.Model;

public class Entity : Observable
{
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
}