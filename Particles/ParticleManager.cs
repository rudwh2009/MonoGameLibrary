using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Particles;
public class ParticleManager<T>
{
    private class CircularParticleArray
    {
        private int start;
        public int Start
        {
            get { return start; }
            set { start = value % list.Length; }
        }
        public int Count { get; set; }
        public int Capacity { get { return list.Length; } }
        private Particle<T>[] list;
        public CircularParticleArray(int capacity)
        {
            list = new Particle<T>[capacity];
        }
        public Particle<T> this[int i]
        {
            get { return list[(start + i) % list.Length]; }
            set { list[(start + i) % list.Length] = value; }
        }
    }

    // This delegate will be called for each particle.
    private readonly Action<Particle<T>, float> updateParticle;
    private CircularParticleArray particleList;
    public ParticleManager(int capacity, Action<Particle<T>> updateParticle)
    {
        this.updateParticle = (p, dt) => updateParticle(p);
        particleList = new CircularParticleArray(capacity);
        // Populate the list with empty particle objects, for reuse. 
        for (int i = 0; i < capacity; i++)
            particleList[i] = new Particle<T>();
    }

    public ParticleManager(int capacity, Action<Particle<T>, float> updateParticle)
    {
        this.updateParticle = updateParticle;
        particleList = new CircularParticleArray(capacity);
        // Populate the list with empty particle objects, for reuse.
        for (int i = 0; i < capacity; i++)
            particleList[i] = new Particle<T>();
    }

    public void CreateParticle(Texture2D texture, Vector2 position, Color tint, float duration, Vector2 scale, T state, float theta = 0, ParticleBlendMode blendMode = ParticleBlendMode.Alpha)
    {
        Particle<T> particle;
        if (particleList.Count == particleList.Capacity)
        {
            // if the list is full, overwrite the oldest particle, and rotate the circular list 
            particle = particleList[0];
            particleList.Start++;
        }
        else
        {
            particle = particleList[particleList.Count];
            particleList.Count++;
        }
        // Create the particle 
        particle.Texture = texture;
        particle.Position = position;
        particle.Tint = tint;
        particle.Duration = duration;
        particle.PercentLife = 1f;
        particle.Scale = scale;
        particle.Orientation = theta;
        particle.State = state;
        particle.BlendMode = blendMode;
    }

    public void Update(float dtSeconds)
    {
        int removalCount = 0;
        for (int i = 0; i < particleList.Count; i++)
        {
            var particle = particleList[i];

            updateParticle(particle, dtSeconds);
            if (particle.Duration > 0f)
                particle.PercentLife -= dtSeconds / particle.Duration;
            else
                particle.PercentLife = -1f;

			// Sift deleted particles to the end of the list.
			// Only swap once we have actually found a dead particle; this avoids a swap on every live particle.
			if (particle.PercentLife < 0)
			{
				removalCount++;
			}
			else if (removalCount != 0)
			{
				Swap(particleList, i - removalCount, i);
			}
        }
        particleList.Count -= removalCount;
    }

    /// <summary>
    /// Removes particles whose positions are farther than <paramref name="maxDistSq"/> from <paramref name="center"/>.
    /// Useful as a cheap camera-based culling step to reduce both update and draw costs.
    /// </summary>
    public void CullByDistanceSquared(Vector2 center, float maxDistSq)
    {
        if (particleList.Count == 0)
            return;
        if (maxDistSq <= 0f)
        {
            particleList.Count = 0;
            return;
        }

        int removalCount = 0;
        for (int i = 0; i < particleList.Count; i++)
        {
            var particle = particleList[i];
            float d2 = Vector2.DistanceSquared(center, particle.Position);
            if (d2 > maxDistSq)
            {
                removalCount++;
            }
            else if (removalCount != 0)
            {
                Swap(particleList, i - removalCount, i);
            }
        }
        particleList.Count -= removalCount;
    }

    public void Update()
    {
        Update(1f / 60f);
    }
    private static void Swap(CircularParticleArray list, int index1, int index2)
    {
		if (index1 == index2)
			return;
        var temp = list[index1];
        list[index1] = list[index2];
        list[index2] = temp;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Texture2D lastTexture = null;
        Vector2 lastOrigin = default;
        for (int i = 0; i < particleList.Count; i++)
        {
            var particle = particleList[i];
            var tex = particle.Texture;
            if (tex != lastTexture)
            {
                lastTexture = tex;
                // Fast-path the common 1x1 pixel texture.
                lastOrigin = (tex.Width == 1 && tex.Height == 1)
                    ? new Vector2(0.5f, 0.5f)
                    : new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);
            }
            spriteBatch.Draw(tex, particle.Position, null, particle.Tint, particle.Orientation, lastOrigin, particle.Scale, 0, 0);
        }
    }

    public void DrawCulled(SpriteBatch spriteBatch, Vector2 center, float maxDistSq)
    {
        Texture2D lastTexture = null;
        Vector2 lastOrigin = default;
        for (int i = 0; i < particleList.Count; i++)
        {
            var particle = particleList[i];
            if (Vector2.DistanceSquared(center, particle.Position) > maxDistSq)
                continue;

            var tex = particle.Texture;
            if (tex != lastTexture)
            {
                lastTexture = tex;
                lastOrigin = (tex.Width == 1 && tex.Height == 1)
                    ? new Vector2(0.5f, 0.5f)
                    : new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);
            }
            spriteBatch.Draw(tex, particle.Position, null, particle.Tint, particle.Orientation, lastOrigin, particle.Scale, 0, 0);
        }
    }

    public void Draw(SpriteBatch spriteBatch, ParticleBlendMode blendMode)
    {
		Texture2D lastTexture = null;
		Vector2 lastOrigin = default;
        for (int i = 0; i < particleList.Count; i++)
        {
            var particle = particleList[i];
            if (particle.BlendMode != blendMode)
                continue;

			var tex = particle.Texture;
			if (tex != lastTexture)
			{
				lastTexture = tex;
				lastOrigin = (tex.Width == 1 && tex.Height == 1)
					? new Vector2(0.5f, 0.5f)
					: new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);
			}
			spriteBatch.Draw(tex, particle.Position, null, particle.Tint, particle.Orientation, lastOrigin, particle.Scale, 0, 0);
        }
    }

    public void DrawCulled(SpriteBatch spriteBatch, ParticleBlendMode blendMode, Vector2 center, float maxDistSq)
    {
        Texture2D lastTexture = null;
        Vector2 lastOrigin = default;
        for (int i = 0; i < particleList.Count; i++)
        {
            var particle = particleList[i];
            if (particle.BlendMode != blendMode)
                continue;
            if (Vector2.DistanceSquared(center, particle.Position) > maxDistSq)
                continue;

            var tex = particle.Texture;
            if (tex != lastTexture)
            {
                lastTexture = tex;
                lastOrigin = (tex.Width == 1 && tex.Height == 1)
                    ? new Vector2(0.5f, 0.5f)
                    : new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);
            }
            spriteBatch.Draw(tex, particle.Position, null, particle.Tint, particle.Orientation, lastOrigin, particle.Scale, 0, 0);
        }
    }
}

