﻿Imports LANS.SystemsBiology.SequenceModel
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic
Imports LANS.SystemsBiology.SequenceModel.NucleotideModels
Imports LANS.SystemsBiology.AnalysisTools.SequenceTools.SequencePatterns.Pattern
Imports LANS.SystemsBiology.AnalysisTools.SequenceTools.SequencePatterns.Topologically.SimilarityMatches
Imports Microsoft.VisualBasic.Language

Namespace Topologically

    ''' <summary>
    ''' === Palindromic hexamers ===
    ''' For a given sequence, any palindrome of 6 nt (e.g., AAATTT) Is given a value of 1, while 
    ''' all bases Not included inpalindromic hexamers are given a value of 0 (van et al. 2003).
    ''' -- van Noort V, Worning P, Ussery DW, Rosche WA, Sinden RR Strand misalignments lead To 
    '''    quasipalindrome correction (2003) 19:365-9
    ''' <see cref="SearchMirror"/> (镜像回文序列)
    ''' 
    ''' 
    ''' === Inverted Repeats ===
    ''' Local Inverted repeats are found by taking a 100 bp sequence window, And looking For the 
    ''' best match Of a 30 bp piece withinthat window, On the opposite strand, In the opposite 
    ''' direction (Jensen et al. 1999). 
    ''' Values can range from 0 (no match at all)To 1 (one Or more perfect match within the window).
    ''' -- L. J. Jensen And C. Friis And D.W. Ussery Three views of complete chromosomes (1999) 150773-777
    ''' <see cref="InvokeSearchReversed"/> (反向重复)
    ''' 
    ''' 
    ''' === Quasi-palindromes ===
    ''' "Quasi-palindromes" are short inverted repeats, which are found by taking a 30 bp piece of sequence, 
    ''' And looking for matcheswith at least 6 out of 7 nt matching, on the opposite strand, in the opposite 
    ''' direction (van et al. 2003). Values canrange from 0 (no match at all) to 1 (one Or more perfect 
    ''' match within the window).
    ''' -- van Noort V, Worning P, Ussery DW, Rosche WA, Sinden RR Strand misalignments lead 
    '''    To quasipalindrome correction (2003) 19:365-9
    ''' <see cref="Topologically.Imperfect"/> (非完全回文)
    ''' 
    ''' 
    ''' === Perfect-palindromes ===
    ''' "Perfect-palindromes" are short inverted repeats, which are found by taking a 30 bp piece of sequence, 
    ''' And looking forperfect matches of 7 nt Or longer, on the opposite strand, in the opposite direction (van et al. 2003). 
    ''' Values can rangefrom 0 (no match at all) to 1 (one Or more perfect match within the window).
    ''' -- van Noort V, Worning P, Ussery DW, Rosche WA, Sinden RR Strand misalignments lead To 
    '''    quasipalindrome correction (2003) 19:365-9
    ''' <see cref="SearchPalindrome"/>  (简单回文)
    ''' 
    ''' === Simple Repeats ===
    ''' A "simple repeat" Is a region which contains a simple oligonucleotide repeat, Like microsattelites. 
    ''' Simple repeats are foundby looking for tandem repeats of length R within a 2R-bp window. 
    ''' By using the values 12, 14, 15, 16, And 18 for R, allsimple repeats of lengths 1 through 9 are calculated, 
    ''' of length of at least 24 bp (Jensen et al. 1999). Values can range from 0(no match at all) to 1 
    ''' (one Or more perfect match within the window).
    ''' -- L. J. Jensen And C. Friis And D.W. Ussery Three views of complete chromosomes (1999) 150773-777
    ''' <see cref="SearchRepeats"/> (简单重复序列)
    ''' 
    ''' === GC Skew ===
    ''' For many genomes there Is a strand bias, such that one strand tends To have more G's, 
    ''' whilst the other strand has more C's.This GC-skew bias can be measured the number of G's 
    ''' minus the number of C's over a fixed length (e.g. 10,000 bp) of DNA(Jensen et al. 1999). 
    ''' The values can range from +1 (all G's on the examined sequence, with all C's on the other strand), 
    ''' to -1(the reverse case - all C's on the examined sequence, and all G's on the other strand). 
    ''' There is a correlation with GC-skewand the replication leading and lagging strands.
    ''' -- L. J. Jensen And C. Friis And D.W. Ussery Three views of complete chromosomes (1999) 150773-777
    ''' 
    ''' === Percent AT ===
    ''' The percent AT Is a running average Of the AT content, over a given window size. Typically For a bacterial 
    ''' genomes Of about5 Mbp, the window size Is 10,000 bp. The Percent AT can range from 0 (no AT content) To 1 (100% AT). 
    ''' The Percent AT iscorrelated With other DNA structural features, such that AT rich regions are often more readily 
    ''' melted, tend To be lessflexible And more rigid, although they can also be readily compacted chromatin proteins (Pedersen et al. 2000).
    ''' -- A.G. Pedersen And L.J. Jensen And H.H. St\aerfeldt And S. Brunak And D.W. 
    '''    Ussery A DNA structural atlas of extitE. coli (2000) 299907-930
    ''' </summary>
    <PackageNamespace("Palindrome.Search",
                      Publisher:="xie.guigang@gcmodeller.org", Url:="http://gcmodeller.org")>
    Public Module Palindrome

        <ExportAPI("Palindrome.Vector")>
        Public Function PalindromeLociVector(DIR As String, Length As Integer) As Double()
            Return Density(Of PalindromeLoci)(DIR, size:=Length)
        End Function

        <ExportAPI("ImperfectPalindrome.Vector")>
        Public Function ImperfectPalindromeVector(DIR As String, length As Integer) As Double()
            Return Density(Of ImperfectPalindrome)(DIR, size:=length)
        End Function

        <ExportAPI("ImperfectPalindrome.Vector.TRIM")>
        Public Function ImperfectPalindromeVector(DIR As String, length As Integer, min As Integer, max As Integer) As Double()
            Call $"Start loading original data from {DIR}".__DEBUG_ECHO
            Dim files = (From file As String
                         In FileIO.FileSystem.GetFiles(DIR, FileIO.SearchOption.SearchTopLevelOnly, "*.csv")
                         Select file.LoadCsv(Of ImperfectPalindrome)).ToArray
            Call $"Data load done! Start to filter data...".__DEBUG_ECHO
            files = (From genome
                     In files.AsParallel
                     Select (From site As ImperfectPalindrome
                             In genome
                             Where site.MaxMatch >= min AndAlso
                                 site.MaxMatch <= max AndAlso
                                 site.Palindrome.Count("-"c) <> site.Palindrome.Length AndAlso
                                 site.Site.Count("-"c) <> site.Site.Length
                             Select site).ToList).ToArray
            Call $"Generates density vector....".__DEBUG_ECHO
            Return Density(Of ImperfectPalindrome)(files, size:=length)
        End Function

        Public Function ToVector(Of TSite As Contig)(sites As IEnumerable(Of TSite), size As Integer) As Integer()
            Dim LQuery = (From i As Integer
                          In size.Sequence
                          Select (From site As TSite
                                  In sites
                                  Where site.MappingLocation.ContainSite(i)
                                  Select 1).FirstOrDefault).ToArray
            Call Console.Write(".")
            Return LQuery
        End Function

        Public Function Density(Of TView As Contig)(DIR As String, size As Integer) As Double()
            Dim files = (From file As String
                         In FileIO.FileSystem.GetFiles(DIR, FileIO.SearchOption.SearchTopLevelOnly, "*.csv")
                         Select file.LoadCsv(Of TView)).ToArray
            Return Density(files, size)
        End Function

        Public Function Density(Of TView As Contig)(genomes As IEnumerable(Of IEnumerable(Of TView)), size As Integer) As Double()
            Dim Vecotrs = (From genome As IEnumerable(Of TView)
                           In genomes.AsParallel
                           Select vector = ToVector(genome, size)).ToArray

            Call New String("="c, 120).__DEBUG_ECHO
            Call $"genomes={Vecotrs.Count}".__DEBUG_ECHO

            Dim p_vectors As Double() = size.ToArray(Function(index As Integer) As Double
                                                         Dim site As Integer() = Vecotrs.ToArray(Function(genome) genome(index))
                                                         Dim hashRepeats = (From g As Double In site.AsParallel Where g > 0 Select g).ToArray
                                                         Dim pHas As Double = hashRepeats.Length / site.Length
                                                         Return pHas
                                                     End Function)
            Return p_vectors
        End Function

        ''' <summary>
        ''' Have mirror repeats?
        ''' </summary>
        ''' <param name="Segment"></param>
        ''' <param name="Sequence"></param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("HasMirror?")>
        Public Function HaveMirror(Segment As String, Sequence As String) As Boolean
            Dim Locations = FindLocation(Sequence, Segment)
            If Locations.IsNullOrEmpty Then
                Return False
            End If

            Dim Mirror As String = New String(Segment.Reverse.ToArray)
            Dim l As Integer = Len(Segment)
            Dim Result = (From loci As Integer
                          In Locations
                          Let ml As Integer = __haveMirror(l, loci, Mirror, Sequence)
                          Where ml > -1
                          Select ml).ToArray
            Return Not Result.IsNullOrEmpty
        End Function

        Private Function __haveMirror(l As Integer, Loci As Integer, Mirror As String, Sequence As String) As Integer
            Dim mrStart As Integer = Loci + l
            Dim mMirr As String = Mid(Sequence, mrStart, l)
            If String.Equals(mMirr, Mirror) Then
                Return mrStart + l
            Else
                Return -1
            End If
        End Function

        ''' <summary>
        ''' 这个函数求解的是绝对相等的
        ''' </summary>
        ''' <param name="Segment"></param>
        ''' <param name="Sequence"></param>
        ''' <returns></returns>
        <ExportAPI("Mirrors.Locis.Get")>
        Public Function CreateMirrors(Segment As String, Sequence As String) As PalindromeLoci()
            Dim Locations As Integer() = FindLocation(Sequence, Segment)

            If Locations.IsNullOrEmpty Then
                Return Nothing
            End If

            Dim Mirror As String = New String(Segment.Reverse.ToArray)
            Dim l As Integer = Len(Segment)
            Dim Result = (From loci As Integer
                          In Locations
                          Let ml As Integer = __haveMirror(l, loci, Mirror, Sequence)
                          Where ml > -1
                          Select loci, ml).ToArray
            Return Result.ToArray(Function(site) New PalindromeLoci With {
                                      .Loci = Segment,
                                      .Start = site.loci,
                                      .PalEnd = site.ml,
                                      .Palindrome = Mirror,
                                      .MirrorSite = Mirror})
        End Function

        <ExportAPI("Palindrome.Locis.Get")>
        Public Function CreatePalindrome(Segment As String, Sequence As String) As PalindromeLoci()
            Dim Locations = FindLocation(Sequence, Segment)
            If Locations.IsNullOrEmpty Then
                Return Nothing
            End If

            Dim rev As String = New String(Segment.Reverse.ToArray)
            Dim Mirror As String = NucleicAcid.Complement(rev)
            Dim l As Integer = Len(Segment)
            Dim Result = (From loci As Integer In Locations
                          Let ml As Integer = __haveMirror(l, loci, Mirror, Sequence)
                          Where ml > -1
                          Select loci, ml).ToArray
            Return Result.ToArray(Function(site) New PalindromeLoci With {
                                      .Loci = Segment,
                                      .Start = site.loci,
                                      .PalEnd = site.ml,
                                      .Palindrome = Mirror,
                                      .MirrorSite = rev})
        End Function

        ''' <summary>
        ''' 搜索序列上面的镜像回文片段
        ''' </summary>
        ''' <param name="Sequence"></param>
        ''' <param name="Min"></param>
        ''' <param name="Max"></param>
        ''' <returns></returns>
        <ExportAPI("Search.Mirror")>
        Public Function SearchMirror(Sequence As I_PolymerSequenceModel,
                                     Optional Min As Integer = 3,
                                     Optional Max As Integer = 20) As PalindromeLoci()
            Dim search As New Topologically.MirrorSearchs(Sequence, Min, Max)
            Call search.InvokeSearch()
            Return search.ResultSet.ToArray
        End Function

        <ExportAPI("Search.Palindrome")>
        Public Function SearchPalindrome(Sequence As I_PolymerSequenceModel,
                                         Optional Min As Integer = 3,
                                         Optional Max As Integer = 20) As PalindromeLoci()
            Dim search As New Topologically.PalindromeSearchs(Sequence, Min, Max)
            Call search.InvokeSearch()
            Return search.ResultSet.ToArray
        End Function

        <ExportAPI("Write.Csv.PalindromeLocis")>
        Public Function SaveResultSet(rs As Generic.IEnumerable(Of PalindromeSearchs), SaveTo As String) As Boolean
            Return rs.SaveTo(SaveTo)
        End Function

        ''' <summary>
        ''' Have Palindrome repeats?
        ''' </summary>
        ''' <param name="Segment"></param>
        ''' <param name="Sequence"></param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("HasPalindrome?")>
        Public Function HavePalindrome(Segment As String, Sequence As String) As Boolean
            Dim Locations As Integer() = FindLocation(Sequence, Segment)

            If Locations.IsNullOrEmpty Then
                Return False
            End If

            Dim Mirror As String =
                NucleicAcid.Complement(New String(Segment.Reverse.ToArray))
            Dim l As Integer = Len(Segment)
            Dim Result As Integer() =
                LinqAPI.Exec(Of Integer) <= From loci As Integer
                                            In Locations
                                            Let ml As Integer =
                                                __haveMirror(l, loci, Mirror, Sequence)
                                            Where ml > -1
                                            Select ml
            Return Not Result.IsNullOrEmpty
        End Function
    End Module
End Namespace