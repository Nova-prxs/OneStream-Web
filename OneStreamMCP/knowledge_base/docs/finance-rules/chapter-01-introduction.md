---
title: "Introduction"
book: "finance-rules"
chapter: 1
start_page: 19
end_page: 22
---

# Introduction

## Why I Wrote This Book

I started my career in 2008, after graduating from Penn State, and working at a Fortune 500 coatings company in Pittsburgh as essentially a glorified intern. About six months into the job, the 2008 financial crisis happened and – like most other large companies – the company I worked for did a massive restructuring, dissolving my department in the process. Luckily for me, an implementation of a CPM product was underway, and they needed someone to sit in a dark room and reconcile data. After two-and-a-half years of working on back-to-back implementations, I realized I had a knack for software and enjoyed project work, so I took a job at a local consulting company. Fast forward a few years, and I am interviewing for OneStream in their first office (above a sporting goods store in Rochester, Michigan). One of the founders, John Von Allmen, plus Steve Mebius (who started the same day as I did), worked as consultants for the software implementation at the Pittsburgh coatings company, too. When I joined OneStream, I was still green but eager to learn and got many opportunities to do so. Working on hundreds of projects (and counting) over my career, I was fortunate to be mentored and coached by the same people who created the software. Since I started at OneStream in 2013, the company has quickly ascended to become the dominant CPM leader. I have seen numerous colleagues – who worked with other software products – see the light, and eventually come over to OneStream. The abrupt rise of OneStream has created a knowledge gap in the industry as experts in legacy products, as well as novices attracted to the OneStream shooting star, are catching up to learn OneStream. While Finance Rules and Calculations are one of the most powerful features of the OneStream platform, they also have the steepest learning curve. I experienced that steep learning curve first-hand and mostly learned the hard way – by making mistakes and good ol’ trial and error. I am writing this book for myself nine years ago when I started at OneStream. I desperately wish I had read this book then; it would have saved me a lot of pain and suffering. I hope that it accomplishes good things for you.

## Who Are You?

I believe the audience for this handbook will fall into a number of categories. My goal in writing this book is to provide something for everyone, whether you are brand new to OneStream or have been working for years.

### The OneStream N00B

Over the years, the OneStream community has exploded from a few dozen evangelical early adopters to thousands of employees, partners, and customers across the globe. If you are one of the hundreds of people just getting into OneStream, this book is written, first and foremost, for you. This book starts with explanations of the foundational concepts before advancing to the more technical coding stuff. I will do my best to break through the complexity and technical jargon and provide clear explanations in plain English.

### The OneStream Veteran

Having worked at OneStream since 2013, I am about as veteran as you can be, and I still learned a lot while writing this book. Both through testing various use cases, and discussions with colleagues about a feature or function I hadn’t used before and was curious about. While this book will start with the absolute basics, it will eventually build to tackle more complex topics and use case examples. It should also satisfy some curiosity around why certain things in OneStream work the way they do, or provide an alternate way of doing something you’ve always done one way. The bottom line is that if I learned something from writing this book, I am hopeful that you will learn something from reading it, no matter how experienced you are.

### The <Insert Competing CPM Software Here> Veteran

This book does not draw comparisons or similarities between how OneStream handles Calculations and how they are handled in other products (besides Excel). The good news is that if you understand Calculations and data structures in other products, you have a good head start and making the transition to OneStream will be easy with the help of this book.

### Everyone Else

Maybe you fall somewhere between beginner and veteran, or have dabbled in writing Finance Rules but aren’t fully comfortable writing them on your own. Or maybe you’ve tiptoed carefully through several projects without having to write a Business Rule and feel it’s about time to learn. I hope this book helps you get over the hump, and you become a proficient rules writer.

## Prerequisites

While this book focuses on the underlying concepts relating to the Finance Engine, Cube data, and Finance Rules, writing Calculations and Business Rules is writing code. This book will not teach you how to code but rather teach how to write OneStream Finance Rules and Calculations using code. At the very minimum, a basic understanding of VB.NET or another object-oriented programming language should be attained to gain the most value out of this book. That being said, you do not need to be an expert or even a half-decent programmer to write OneStream Calculations. Your author proudly admits that he has had no formal coding training, and is mostly self-taught. I hope this serves as an inspiration to the non-programmers out there. While there is a plethora of VB.NET and other programming courses available online, the link below is a recommended resource that will help you gain a basic understanding of the VB.NET Framework. https://msdn.microsoft.com/en-us/library/2x7h1hfk.aspx

## Drowning In A Sea Of Options

OneStream offers an enormous variety of choices when it comes to writing Calculations. From where and how they are written to how they can be executed. My goal is to help you make sense of those choices and provide some guide rails based on my experiences. You can certainly veer off the paved road (in some instances, it is necessary!), but staying on the path will get you to your destination the vast majority of the time. I say this as a disclaimer… the way I show things may not be the only way. If you’ve written Calculations that look vastly different than mine, it does not necessarily mean they are wrong.

## Scope Of The Book

As will be explained in the first chapter, this book is focused solely on the OneStream Finance Engine and how to write and execute Calculations and Finance Rules. OneStream has a lot of functionality, so writing Business Rules in other OneStream Engines will be saved as topics for other books.

## Code References

Throughout this book, code snippets for rule examples will be shown and, in some cases, broken down line by line with detailed explanations. Screenshots of the code as it is presented in the OneStream Business Rule Editor are used instead of showing the code as plain text. The benefit is that the screenshotted code is much more readable as various keywords, functions, and comments are color-coded and formatted exactly how they are presented within the product. The disadvantage is that the code cannot be copied and pasted from the book PDF version, and the code can sometimes be squeezed down to fit on the printed page. To account for this, a full application with all referenced code examples (and more) will be available to download at www.OneStreamPress.com/FRC In this application, all rules and scripts can be accessed and executed against real data, and results can be matched up against what is shown in the book. Learning technical concepts, especially involving code, can be challenging when only reading about it. Real learning takes place while doing it. I encourage you to use the example application in parallel with reading the book. Try to change the code, break it, and get it working again. Don’t be afraid to get your hands dirty!

## The Reference Application

The accompanying reference application (noted above) will be for a fictional company, run by The Three Stooges (Moe, Larry, and Curly), called StoogeCorp. StoogeCorp has several companies under its umbrella, including ACME Exterminators, Gypsum Good Antiques, Gottrox Jewelry, and Cheatum Investments. These companies have interesting operations that make for some hopefully interesting use cases for Calculations. The Three Stooges was (and still is) one of my favorite shows, and I was happy to include watching reruns as part of my book ‘research.’
