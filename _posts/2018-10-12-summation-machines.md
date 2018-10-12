---
layout: post
title: "Summation Machines"
categories: math
author: "Christopher J. Adkins"
tags: [Summation,Machines,Math]
usemathjax: true
comments: true
---

A while ago I was watching some of [Carl Bender's](http://physics.wustl.edu/cmb/) physics lectures online and he was talking about the power of summation, specially summation of infinite sequence. There are many different ways to sum infinite sequences, some that make sense in typical way, and some that don't. In any intro undergraduate class you'll be introduced to the concept of infinite series, and you'll learn about convergent and divergent series. Let's say we have an infinite sequence $$\{a_n\}_{n\geq 0} $$ where $$a_n$$ lives in some ring $$R$$, and we have the series

$$ \sum_{n=0}^\infty a_n = a_0 + a_1 + a_2 + a_3 + \ldots = \quad ?$$

It always seemed strange to have the disconnect between the $$\Sigma$$ and the left, and the $$+$$'s on the right. In a more general setting, you can play with this definition in a funny way with a few definition's of what $$\Sigma$$ can do. Let's define an operator $$\Sigma$$ that acts on an infinite sequence with the following properties.

$$ \sum \{a_n\}_{n\geq 0} = a_0 + \sum \{a_n\}_{n\geq 1}  \label{1} \tag{seperation} $$

$$ \sum \{b \cdot a_n\}_{n\geq 0} = b \cdot \sum \{a_n\}_{n\geq 0} \label{2} \tag{distributivity}$$

These are fairly normal things to expect from your addition and multiplication operators. Quite easily now we can construct values of what the sum of some sequences are if we fix down some numbers. The first example most people are shown is the geometric series. We take $$a_n = a r^n$$ where $$a,r \in \mathbb{C}$$. We can easily see using the two properties above that

$$ \sum \{a r^n\}_{n\geq 0} = a + r \cdot \sum \{a r^n\}_{n\geq 0} $$

Rearranging the equation and factoring the sum gives us the following equation

$$  \sum \{a r^n\}_{n\geq 0}  =  \frac {a}{1-r} $$

Algebraically we've found a formula to give us a value for this infinite series. Setting aside the notation of convergence, we've now obtained a representation of the value of the sum. We can even do this to some sequences to assign a value to them under the operator $$\Sigma$$. Take $$\{ (-1)^n \}_{n\geq 0}$$. Via the formula we've derived, we now see ($$a=1,r=-1$$)

$$ \sum \{ (-1)^n \}_{n\geq 0} = \frac{1}{2} $$

Which is interesting since assuming associativity holds, we can group the sum in the following ways

$$ \sum \{ (-1)^n \}_{n\geq 0} = \underbrace{1 - 1}_{ =0}  \underbrace{ +1 - 1}_{ =0}  \underbrace{+1 - 1}_{ =0} + \ldots = 0 + 0 + 0 + \ldots  =0 $$

$$ \sum \{ (-1)^n \}_{n\geq 0} = 1 +\underbrace{(-1 +1)}_{ =0} +\underbrace{(-1 +1)}_{ =0}  + \ldots = 1 + 0 + 0  + \ldots  = 1 $$

It would seem our summation operator $$\Sigma$$ some how knew of the 2 possible values for the sum and decided to average them. You can even do this for sequences that don't seem to converge to anything and get a representation of their sum. Take $$\{ 2^n \}_{n\geq 0}$$, we see that (via the formula, $$a=1,r=2$$)

$$ \sum \{ 2^n \}_{n\geq 0} = -1 $$

It seems that this divergent series has a happy value of $$-1$$ that it wants to represent itself as. I'll follow up this abstracted series stuff in another post later!