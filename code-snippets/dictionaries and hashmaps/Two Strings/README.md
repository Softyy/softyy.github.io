Given two strings, determine if they share a common substring. A substring may be as small as one character.

For example, the words "a", "and", "art" share the common substring . The words "be" and "cat" do not share a substring.

## Function Description

Complete the function twoStrings in the editor below. It should return a string, either YES or NO based on whether the strings share a common substring.

twoStrings has the following parameter(s):

- s1, s2: two strings to analyze.

## Input Format

The first line contains a single integer , the number of test cases.

The following pairs of lines are as follows:

- The first line contains string $$s1$$.
- The second line contains string $$s2$$.

## Constraints

- $$s1$$ and $$s2$$ consist of characters in the range ascii[a-z].
- $$1 \leq p leq 10 $$
- $$1 \leq |s1|,|s2| \leq 10^5$$

## Output Format

For each pair of strings, return YES or NO.

## Sample Input

```
2
hello
world
hi
world
```

## Sample Output

```
YES
NO
```

## Explanation

We have pairs to check:

- $$s1="hello",s2="world"$$. The substrings $$o$$ and $$l$$ are common to both strings.
- $$a="hi",b="world"$$. $$s1$$ and $$s2$$ share no common substrings.
