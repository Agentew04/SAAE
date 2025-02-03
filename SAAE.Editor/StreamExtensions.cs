﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SAAE.Editor;

public static class StreamExtensions {
    // https://stackoverflow.com/questions/20661652/progress-bar-with-httpclient
    public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long> progress = null, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(source);
        if (!source.CanRead)
            throw new ArgumentException("Has to be readable", nameof(source));
        ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite)
            throw new ArgumentException("Has to be writable", nameof(destination));
        ArgumentOutOfRangeException.ThrowIfNegative(bufferSize);

        byte[] buffer = new byte[bufferSize];
        long totalBytesRead = 0;
        int bytesRead;
        while ((bytesRead = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) != 0) {
            await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
            totalBytesRead += bytesRead;
            progress?.Report(totalBytesRead);
        }
    }
}