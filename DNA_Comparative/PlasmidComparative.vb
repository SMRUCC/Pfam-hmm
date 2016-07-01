﻿Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.genomics.Assembly.NCBI.GenBank.CsvExports
Imports SMRUCC.genomics.Interops.NCBI.Extensions.Analysis
Imports SMRUCC.genomics.SequenceModel.FASTA
Imports SMRUCC.genomics.SequenceModel.NucleotideModels

''' <summary>
''' 根据BBH结果所计算出来的保守片段之间进行delta值的相互比较
''' </summary>
''' <remarks></remarks>
''' 
<[Namespace]("Comparative.Plasmid")>
Public Module PlasmidComparative

    <ExportAPI("Plasmid.Partitioning")>
    Public Function PlasmidPartitioning(Besthits As BestHit, CdsInfo As IEnumerable(Of GeneDumpInfo), Fasta As FastaToken) As PartitioningData()
        Dim ConservedRegions = Besthits.GetConservedRegions
        Dim reader As New SegmentReader(Fasta, False)
        Dim ORF = (From gene As GeneDumpInfo
                   In CdsInfo
                   Select gene
                   Group By gene.LocusID Into Group) _
                         .ToDictionary(Function(gene) gene.LocusID,
                                       Function(gene) gene.Group.First)
        Dim Regions As List(Of String()) =
            New List(Of String())(ConservedRegions) + From id As String
                                                      In Besthits.GetUnConservedRegions(ConservedRegions)
                                                      Select New String() {id}
        Dim LQuery As PartitioningData() =
            LinqAPI.Exec(Of PartitioningData) <= From ls As String()
                                                 In Regions
                                                 Let pos As Integer() = (From id As String
                                                                         In ls
                                                                         Let nn As GeneDumpInfo = ORF(id)
                                                                         Select {nn.Left, nn.Right}).MatrixToVector
                                                 Let left As Integer = pos.Min
                                                 Let right As Integer = pos.Max
                                                 Select New PartitioningData With {
                                                     .GenomeID = Fasta.Title,
                                                     .ORFList = ls,
                                                     .PartitioningTag = String.Join(", ", ls),
                                                     .LociLeft = left,
                                                     .LociRight = right,
                                                     .SequenceData = reader.GetSegmentSequence(left, right)
                                                 }
        Return LQuery.OrderBy(Function(x) x.PartitioningTag).ToArray
    End Function

    <ExportAPI("Plasmid.DeltaMatrix")>
    Public Function CreateDeltaMatrix(partitions As IEnumerable(Of PartitioningData)) As File
        Dim df As New DocumentStream.File
        Dim cache = (From part As PartitioningData In partitions Select CacheData = New NucleicAcid(part.SequenceData), part).ToArray ' 因为要保持一一对应关系，所以这里不可以使用并行化拓展了
        Dim y As NucleicAcid() = cache.ToArray(Function(x) x.CacheData)

        df += ("X/Y" + (From part As PartitioningData In partitions Select part.PartitioningTag).ToList)
        df += From x In cache
              Let cols As List(Of String) =
                  __generateCols(x.CacheData, y)
              Select __row(x.part, cols) ' 因为要保持一一对应关系，所以这里不可以使用并行化拓展了
        Return df
    End Function

    Private Function __generateCols(x As NucleicAcid, cache As IEnumerable(Of NucleicAcid)) As List(Of String)
        Return LinqAPI.MakeList(Of String) <= From y As NucleicAcid
                                              In cache
                                              Let n As Double = 1000 * DNA_Comparative.Sigma(x, y)
                                              Select CStr(CInt(n))
    End Function

    Private Function __row(item As PartitioningData, cols As List(Of String)) As RowObject
        Dim row As New RowObject(item.PartitioningTag + cols)
        Return row
    End Function
End Module
