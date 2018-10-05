---
layout: post
title: "The Riemann Hypothesis solved?"
categories: math
author: "Christopher J. Adkins"
meta: "Riemann Hypothesis math"
usemathjax: true
comments: true
---
As you may or may not have heard, [Michael Atiyah](https://en.wikipedia.org/wiki/Michael_Atiyah) has recently put out a short paper outlining his proof of the Riemann Hypothesis. As I was curious if it was true, I decided to check out [his proof](/assets/posts/2018-The_Riemann_Hypothesis.pdf). The initial doubt of the validity of this proof came about since you'll see almost no reference to the properties of Zeta function itself. I won't go into the gossip, of if it's been solved or not, you can find that all over the internet.

Let's have a quick crash course on the Riemann Zeta function. Back in the day Euler was studying the following power series:

$$ \zeta(s) = 1 + \frac{1}{2^s} + \frac{1}{3^s} + \frac{1}{4^s} + \frac{1}{5^s} + \ldots = \sum_{n=1}^\infty \frac{1}{n^s} $$

where $$s>1$$ since $$s=1$$ is every calculus students favourite series, the harmonic series, which diverges. He also noticed that this series related to the prime number in a peculiar way, namely he found the following form of this series.

$$ \zeta(s) = \prod_{p} \frac{1}{1-p^{-s}} $$

Which you can deduce looking at $$\zeta(s)$$ and $$2^{-s} \zeta(s)$$, and subtracting them to see:

$$\left ( 1 - 2^{-s} \right) \zeta(s) = \underbrace{1 + \frac{1}{3^s} + \frac{1}{5^s} + \frac{1}{7^s} + \frac{1}{9^s} + \ldots}_{\text{no factors of 2 in denominator}} $$

Continuing the pattern, we can remove the factors of 3 with the same trick:

$$\left ( 1 - 3^{-s} \right) \left ( 1 - 2^{-s} \right) \zeta(s) = \underbrace{1 + \frac{1}{5^s} + \frac{1}{7^s} + \frac{1}{11^s} + \frac{1}{13^s} + \ldots}_{\text{no factors of 2 or 3 in denominator}} $$

If we continued this using prime numbers, we can now easily see (maybe using the fundamental theorem of arithmetic for reference)

$$ \zeta(s) \prod_p \left ( 1 - p^{-s}\right ) = 1 $$

History moved on and complex analysis became gave mathematicians the ability to turn cheat codes on. People were able to analytically extend functions to the complex plane $$\mathbb{C}$$ and Riemann found a helpful functional equation to extend $$\zeta$$ of the real line:

$$   \zeta(s) = 2^s \pi^{s-1} \sin \left ( \frac{ \pi s }{2} \right )  \Gamma(1-s) \zeta(1-s) $$

where $$\Gamma$$ is the Gamma function (generalization of the factorial, $$\Gamma(n) = (n-1)!$$ when $$n \in \mathbb{N}$$). If you've ever studied complex analysis, you'll know that the zeros of a analytic function are the bread of butter to understanding it. Aside from the trivial zeros of sine in the functional equation, the goal was to find out the zeros. Riemann hypothesised that all the zeros lie on the line of $$s= \frac{1}{2} + i y$$. Why that line? Well due to the symmetric under tones of the functional equation!  