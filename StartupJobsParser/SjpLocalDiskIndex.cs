using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using Lns = Lucene.Net.Store;

namespace StartupJobsParser
{
    public class SjpLocalDiskIndex : ISjpIndex
    {
        public string IndexDirPath { get; private set; }

        private DirectoryInfo _directoryInfo;
        private static volatile object _indexWriterMutex = new object();

        public SjpLocalDiskIndex(string indexDirPath)
        {
            if (!Directory.Exists(indexDirPath))
            {
                Directory.CreateDirectory(indexDirPath);
            }

            IndexDirPath = indexDirPath;

            _directoryInfo = new DirectoryInfo(indexDirPath);
        }

        public void AddToIndex(JobDescription jd)
        {
            using (Lns.SimpleFSDirectory luceneDir = new Lns.SimpleFSDirectory(_directoryInfo))
            {
                using (Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
                {
                    // Lucene only allow a single writer
                    lock (_indexWriterMutex)
                    {
                        using (IndexWriter indexWriter =
                            new IndexWriter(luceneDir, analyzer, Lucene.Net.Index.IndexWriter.MaxFieldLength.UNLIMITED))
                        {
                            Document doc = new Document();
                            doc.Add(new Field("Uid", jd.Uid, Field.Store.YES, Field.Index.ANALYZED));
                            doc.Add(new Field("Company", jd.Company, Field.Store.YES, Field.Index.ANALYZED));
                            doc.Add(new Field("Title", jd.Title, Field.Store.YES, Field.Index.ANALYZED));
                            doc.Add(new Field("Location", jd.Location, Field.Store.YES, Field.Index.ANALYZED));
                            doc.Add(new Field("FullTextDescription", jd.FullTextDescription, Field.Store.NO, Field.Index.ANALYZED));
                            doc.Add(new Field("FullHtmlDescription", jd.FullHtmlDescription, Field.Store.YES, Field.Index.NO));
                            doc.Add(new Field("StorageUri", jd.StorageUri, Field.Store.YES, Field.Index.NO));
                            indexWriter.AddDocument(doc);
                            indexWriter.Commit();
                        }
                    }
                }
            }
        }

        public void RemoveFromIndex(string uid)
        {
            using (Lns.SimpleFSDirectory luceneDir = new Lns.SimpleFSDirectory(_directoryInfo))
            {
                using (Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
                {
                    using (IndexWriter indexWriter =
                        new IndexWriter(luceneDir, analyzer, Lucene.Net.Index.IndexWriter.MaxFieldLength.UNLIMITED))
                    {
                        Query query = new Lucene.Net.Search.TermQuery(new Term("Uid", uid));
                        indexWriter.DeleteDocuments(query);
                        indexWriter.Commit();
                    }
                }
            }
        }

        public IEnumerable<JobDescription> FindJds(string term)
        {
            using (Lns.SimpleFSDirectory luceneDir = new Lns.SimpleFSDirectory(_directoryInfo))
            {
                using (Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
                {
                    using (Searcher indexSearcher = new IndexSearcher(luceneDir))
                    {
                        Query query = new Lucene.Net.Search.TermQuery(new Term("FullDescription", term));
                        TopDocs results = indexSearcher.Search(query, 100);
                        for (int i = 0; i < results.TotalHits; i++)
                        {
                            Document doc = indexSearcher.Doc(results.ScoreDocs[i].Doc);
                            JobDescription jd;
                            using (FileStream fs = new FileStream(doc.Get("StorageUri"), FileMode.Open, FileAccess.Read))
                            {
                                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(JobDescription));
                                jd = ser.ReadObject(fs) as JobDescription;
                            }
                            yield return jd;
                        }
                    }
                }
            }
        }
    }
}