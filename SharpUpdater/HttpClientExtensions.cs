using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SharpUpdater {

    // https://stackoverflow.com/questions/20661652
    public static class StreamExtensions {

        public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long>? progress = null, CancellationToken cancellationToken = default) {
            ArgumentNullException.ThrowIfNull(source);
            if (!source.CanRead)
                throw new ArgumentException("Has to be readable", nameof(source));
            ArgumentNullException.ThrowIfNull(destination);
            if (!destination.CanWrite)
                throw new ArgumentException("Has to be writable", nameof(destination));
            ArgumentOutOfRangeException.ThrowIfNegative(bufferSize);

            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false)) != 0) {
                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                progress?.Report(totalBytesRead);
            }
        }
    }

    public static class HttpClientExtensions {

        /// <summary>
        /// 下载文件（支持断点续传）
        /// </summary>
        public static async Task DownloadAsync(this HttpClient client, Uri requestUri, Stream destination, IProgress<float>? progress = null, CancellationToken cancellationToken = default) {
            using (var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken)) {
                response.EnsureSuccessStatusCode();
                var contentLength = response.Content.Headers.ContentLength;

                using (var download = await response.Content.ReadAsStreamAsync(cancellationToken)) {
                    if (progress == null || !contentLength.HasValue) {
                        await download.CopyToAsync(destination, cancellationToken);
                        return;
                    }

                    var relativeProgress = new Progress<long>(totalBytes => progress.Report((float)totalBytes / contentLength.Value));
                    await download.CopyToAsync(destination, 81920, relativeProgress, cancellationToken);
                    progress.Report(1);
                }
            }
        }

        /// <summary>
        /// 下载文件（支持断点续传）
        /// </summary>
        /// <param name="client">HttpClient 实例</param>
        /// <param name="requestUri">请求 URI</param>
        /// <param name="destination">目标流</param>
        /// <param name="downloadedBytes">已下载的字节数（用于断点续传）</param>
        /// <param name="progress">进度报告</param>
        /// <param name="cancellationToken">取消令牌</param>
        public static async Task DownloadWithResumeAsync(
            this HttpClient client,
            Uri requestUri,
            Stream destination,
            long downloadedBytes,
            IProgress<float>? progress = null,
            CancellationToken cancellationToken = default) {

            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri)) {
                // 设置 Range 请求头
                if (downloadedBytes > 0) {
                    request.Headers.Range = new RangeHeaderValue(downloadedBytes, null);
                }

                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)) {
                    response.EnsureSuccessStatusCode();

                    var contentLength = response.Content.Headers.ContentLength;
                    var totalBytes = downloadedBytes + (contentLength ?? 0);

                    // 如果服务器不支持断点续传，返回的状态码是 OK 而不是 PartialContent
                    var isResumeSupported = response.StatusCode == System.Net.HttpStatusCode.PartialContent;

                    using (var download = await response.Content.ReadAsStreamAsync(cancellationToken)) {
                        if (progress == null || totalBytes == 0) {
                            await download.CopyToAsync(destination, cancellationToken);
                            return;
                        }

                        var relativeProgress = new Progress<long>(bytesRead => {
                            var totalDownloaded = isResumeSupported ? downloadedBytes + bytesRead : bytesRead;
                            progress.Report((float)totalDownloaded / totalBytes);
                        });

                        await download.CopyToAsync(destination, 81920, relativeProgress, cancellationToken);
                        progress.Report(1);
                    }
                }
            }
        }

        /// <summary>
        /// 检查服务器是否支持断点续传
        /// </summary>
        public static async Task<bool> CheckResumeSupportAsync(this HttpClient client, Uri requestUri) {
            try {
                using (var request = new HttpRequestMessage(HttpMethod.Head, requestUri)) {
                    using (var response = await client.SendAsync(request)) {
                        var acceptRanges = response.Headers.AcceptRanges;
                        return acceptRanges.Contains("bytes");
                    }
                }
            } catch {
                return false;
            }
        }
    }
}
