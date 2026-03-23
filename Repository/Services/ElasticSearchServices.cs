using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using Repository.Models;

namespace Repository.Services
{
    public class ElasticSearchServices
    {
        private readonly ElasticsearchClient _client;


        public ElasticSearchServices(IConfiguration config)
        {
            var settings = new ElasticsearchClientSettings(new Uri(config["Elasticsearch:Uri"]))
                .Authentication(new BasicAuthentication(
                    config["Elasticsearch:Username"],
                    config["Elasticsearch:Password"]
                ))
                .DefaultIndex("queries");

            _client = new ElasticsearchClient(settings);
        }

        // 🔹 Index Data (Insert / Update)
        public async Task IndexQueryAsync(t_Query query)
        {
            Console.WriteLine(" Indexing called");

            var response = await _client.IndexAsync(query, i => i
                .Index("queries")
                .Id(query.c_QueryId)
            );

            if (!response.IsValidResponse)
            {
                Console.WriteLine(" ERROR:");
                Console.WriteLine(response.DebugInformation);
            }
            else
            {
                Console.WriteLine("✅ Indexed: " + query.c_QueryId);
            }
        }

        public async Task DeleteQueryAsync(int id)
        {
            await _client.DeleteAsync<t_Query>(id, d => d.Index("queries"));
        }

        // 🔹 Search by Title
        // public async Task<List<t_Query>> SearchQueryAsync(string keyword)
        // {
        //     var response = await _client.SearchAsync<t_Query>(s => s
        //         .Query(q => q
        //             .Match(m => m
        //                 .Field(f => f.c_Title)
        //                 .Query(keyword)
        //             )
        //         )
        //     );

        //     return response.Documents.ToList();
        // }

        // public async Task<List<t_Query>> SearchQueryAsync(string keyword)
        // {
        //     var response = await _client.SearchAsync<t_Query>(s => s
        //         .Query(q => q
        //             .Bool(b => b
        //                 .Should(

        //                     // Fuzzy search (Title + Description)
        //                     sh => sh.MultiMatch(mm => mm
        //                         .Fields(new[] { "c_Title", "c_Description" }) // ✅ FIX
        //                         .Query(keyword)
        //                         .Fuzziness(new Fuzziness("AUTO"))
        //                     ),

        //                     //Partial typing (2-3 letters)
        //                     sh => sh.MultiMatch(mm => mm
        //                         .Fields(new[] { "c_Title", "c_Description" }) // ✅ FIX
        //                         .Query(keyword)
        //                         .Type(TextQueryType.BoolPrefix)
        //                     )
        //                 )
        //                 .MinimumShouldMatch(1)
        //             )
        //         )
        //     );

        //     return response.Documents.ToList();
        // }



        // public async Task<List<t_Query>> SearchQueryAsync(string keyword)
        // {
        //     var response = await _client.SearchAsync<t_Query>(s => s
        //         .Query(q => q
        //             .Bool(b => b
        //                 .Should(

        //                     // ✅ Partial typing (best for "a", "ab")
        //                     sh => sh.MultiMatch(mm => mm
        //                         .Fields(new[] { "c_Title", "c_Description" })
        //                         .Query(keyword)
        //                         .Type(TextQueryType.BoolPrefix)
        //                     ),

        //                     // ✅ Fuzzy (typo handling)
        //                     sh => sh.MultiMatch(mm => mm
        //                         .Fields(new[] { "c_Title", "c_Description" })
        //                         .Query(keyword)
        //                         .Fuzziness(new Fuzziness("AUTO"))
        //                     ),

        //                     // ✅ Force contains (LIKE %keyword%)
        //                     sh => sh.QueryString(qs => qs
        //                         .Fields(new[] { "c_Title", "c_Description" })
        //                         .Query($"*{keyword}*")
        //                     )
        //                 )
        //                 .MinimumShouldMatch(1)
        //             )
        //         )
        //     );

        //     return response.Documents.ToList();
        // }


  public async Task<List<t_Query>> SearchQueryAsync(string keyword, int userId)
        {
            keyword = keyword?.Trim() ?? "";

            var response = await _client.SearchAsync<t_Query>(s => s
                .Query(q => q
                    .Bool(b => b
                        .Must(
                            mu => mu.Term(t => t
                                .Field(f => f.c_UserId)
                                .Value(userId)
                            )
                        )
                        .Should(
                            sh => sh.MultiMatch(mm => mm
                                .Fields(new[] { "c_Title", "c_Description" })
                                .Query(keyword)
                                .Type(TextQueryType.BoolPrefix)
                            ),
                            sh => sh.MultiMatch(mm => mm
                                .Fields(new[] { "c_Title", "c_Description" })
                                .Query(keyword)
                                .Fuzziness(new Fuzziness("AUTO"))
                            ),
                            sh => sh.Wildcard(w => w
                                .Field("c_Title")
                                .Value($"*{keyword.ToLower()}*")
                                .CaseInsensitive(true)
                            ),
                            sh => sh.Wildcard(w => w
                                .Field("c_Description")
                                .Value($"*{keyword.ToLower()}*")
                                .CaseInsensitive(true)
                            )
                        )
                        .MinimumShouldMatch(1)
                    )
                )
            );

            if (!response.IsValidResponse)
            {
                Console.WriteLine("❌ SearchQueryAsync ERROR:");
                Console.WriteLine(response.DebugInformation);
                return new List<t_Query>();
            }

            return response.Documents.ToList();
        }


        public async Task CreateIndexAsync()
        {
            var exists = await _client.Indices.ExistsAsync("queries");

            if (!exists.Exists)
            {
                await _client.Indices.CreateAsync("queries");
                Console.WriteLine("✅ Index created");
            }
        }


        public async Task<List<t_Query>> FilterByDate(DateTime? fromDate, DateTime? toDate)
        {
            var response = await _client.SearchAsync<t_Query>(s => s
                .Query(q => q
                    .Range(r => r
                        .DateRange(dr => dr
                            .Field(f => f.c_QueryDate)
                            .Gte(fromDate)
                            .Lte(toDate)
                        )
                    )
                )
            );

            return response.Documents.ToList();
        }


        // public async Task<List<t_Query>> AdminSearchAsync(string keyword, string status, DateTime? fromDate, DateTime? toDate)
        // {
        //     var mustQueries = new List<Action<QueryDescriptor<t_Query>>>();

        //     // 🔍 Title + Description
        //     if (!string.IsNullOrEmpty(keyword))
        //     {
        //         mustQueries.Add(m => m.MultiMatch(mm => mm
        //         .Fields(new[] { "c_Title", "c_Description" })
        //         .Query(keyword)
        //         .Fuzziness(new Fuzziness("AUTO"))
        //         ));
        //     }

        //     // 📅 Date Filter
        //     if (fromDate.HasValue || toDate.HasValue)
        //     {
        //         mustQueries.Add(m => m.Range(r => r
        //             .DateRange(dr => dr
        //                 .Field("c_QueryDate")
        //                 .Gte(fromDate)
        //                 .Lte(toDate)
        //             )
        //         ));
        //     }

        //     // 📌 Status Filter
        //     if (!string.IsNullOrEmpty(status))
        //     {
        //         mustQueries.Add(m => m.Term(t => t
        //             .Field("c_Status.keyword")
        //             .Value(status)
        //         ));
        //     }

        //     var response = await _client.SearchAsync<t_Query>(s => s
        //         .Query(q => q
        //             .Bool(b => b
        //                 .Must(mustQueries.ToArray())
        //             )
        //         )
        //     );

        //     return response.Documents.ToList();
        // }


        // public async Task<List<t_Query>> AdminSearchAsync(string keyword, string status, DateTime? fromDate, DateTime? toDate)
        // {
        //     var mustQueries = new List<Action<QueryDescriptor<t_Query>>>();

        //     // 🔍 Title + Description
        //     // if (!string.IsNullOrEmpty(keyword))
        //     // {
        //     //     mustQueries.Add(m => m.MultiMatch(mm => mm
        //     //     .Fields(new[] { "c_Title", "c_Description" })
        //     //     .Query(keyword)
        //     //     .Fuzziness(new Fuzziness("AUTO"))
        //     //     ));
        //     // }

        //     if (!string.IsNullOrEmpty(keyword))
        //     {
        //         mustQueries.Add(m => m.Bool(b => b
        //             .Should(

        //                 sh => sh.MultiMatch(mm => mm
        //                     .Fields(new[] { "c_Title", "c_Description" })
        //                     .Query(keyword)
        //                     .Type(TextQueryType.BoolPrefix)
        //                 ),

        //                 sh => sh.MultiMatch(mm => mm
        //                     .Fields(new[] { "c_Title", "c_Description" })
        //                     .Query(keyword)
        //                     .Fuzziness(new Fuzziness("AUTO"))
        //                 ),

        //                 sh => sh.QueryString(qs => qs
        //                     .Fields(new[] { "c_Title", "c_Description" })
        //                     .Query($"*{keyword}*")
        //                 )
        //             )
        //             .MinimumShouldMatch(1)
        //         ));
        //     }

        //     // 📅 Date Filter
        //     if (fromDate.HasValue || toDate.HasValue)
        //     {
        //         mustQueries.Add(m => m.Range(r => r
        //             .DateRange(dr => dr
        //                 .Field("c_QueryDate")
        //                 .Gte(fromDate)
        //                 .Lte(toDate)
        //             )
        //         ));
        //     }

        //     // 📌 Status Filter
        //     if (!string.IsNullOrEmpty(status))
        //     {
        //         mustQueries.Add(m => m.Term(t => t
        //             .Field("c_Status.keyword")
        //             .Value(status)
        //         ));
        //     }

        //     var response = await _client.SearchAsync<t_Query>(s => s
        //         .Query(q => q
        //             .Bool(b => b
        //                 .Must(mustQueries.ToArray())
        //             )
        //         )
        //     );

        //     return response.Documents.ToList();
        // }


public async Task<List<t_Query>> AdminSearchAsync(string keyword, string status, DateTime? fromDate, DateTime? toDate)
        {
            var mustQueries = new List<Action<QueryDescriptor<t_Query>>>();

            // 🔍 Title + Description
            // if (!string.IsNullOrEmpty(keyword))
            // {
            //     mustQueries.Add(m => m.MultiMatch(mm => mm
            //     .Fields(new[] { "c_Title", "c_Description" })
            //     .Query(keyword)
            //     .Fuzziness(new Fuzziness("AUTO"))
            //     ));
            // }

            if (!string.IsNullOrEmpty(keyword))
            {
                mustQueries.Add(m => m.Bool(b => b
                    .Should(

                        sh => sh.MultiMatch(mm => mm
                            .Fields(new[] { "c_Title", "c_Description" })
                            .Query(keyword)
                            .Type(TextQueryType.BoolPrefix)
                        ),

                        sh => sh.MultiMatch(mm => mm
                            .Fields(new[] { "c_Title", "c_Description" })
                            .Query(keyword)
                            .Fuzziness(new Fuzziness("AUTO"))
                        ),

                        sh => sh.QueryString(qs => qs
                            .Fields(new[] { "c_Title", "c_Description" })
                            .Query($"*{keyword}*")
                        )
                    )
                    .MinimumShouldMatch(1)
                ));
            }

            // 📅 Date Filter
            if (fromDate.HasValue || toDate.HasValue)
            {
                mustQueries.Add(m => m.Range(r => r
                    .DateRange(dr => dr
                        .Field("c_QueryDate")
                        .Gte(fromDate)
                        .Lte(toDate)
                    )
                ));
            }

            // 📌 Status Filter
            if (!string.IsNullOrEmpty(status))
            {
                mustQueries.Add(m => m.Term(t => t
                    .Field("c_Status.keyword")
                    .Value(status)
                ));
            }

            var response = await _client.SearchAsync<t_Query>(s => s
                .Query(q => q
                    .Bool(b => b
                        .Must(mustQueries.ToArray())
                    )
                )
            );

            return response.Documents.ToList();
        }

    }
}