import { useState } from 'react';

const STAR_PATH =
  'M12 2.5l2.94 6.1 6.7.9-4.9 4.67 1.2 6.63L12 17.6l-5.94 3.2 1.2-6.63L2.36 9.5l6.7-.9L12 2.5z';

function Star({ fill }) {
  return (
    <span className="star">
      <svg className="star-base" viewBox="0 0 24 24" aria-hidden="true">
        <path d={STAR_PATH} />
      </svg>
      <span className="star-fill" style={{ width: `${fill * 100}%` }}>
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <path d={STAR_PATH} />
        </svg>
      </span>
    </span>
  );
}

/**
 * Five stars with half-star precision. `value` is 1-10 (or null for unrated).
 * Pass `onChange` to make it interactive; clicking the current value clears the rating.
 */
export default function StarRating({ value, onChange, size = 20, label = 'Rating' }) {
  const [hover, setHover] = useState(null);
  const interactive = typeof onChange === 'function';
  const shown = hover ?? value ?? 0;

  const choose = (next) => onChange(value === next ? null : next);

  return (
    <span
      className={`stars${interactive ? ' interactive' : ''}`}
      style={{ '--star-size': `${size}px` }}
      role={interactive ? 'group' : 'img'}
      aria-label={value ? `${label}: ${value / 2} out of 5 stars` : `${label}: not rated`}
      onMouseLeave={() => setHover(null)}
    >
      {[1, 2, 3, 4, 5].map((i) => (
        <span className="star-slot" key={i}>
          <Star fill={Math.max(0, Math.min(1, shown / 2 - (i - 1)))} />
          {interactive && (
            <>
              <button
                type="button"
                className="half left"
                aria-label={`${i - 0.5} stars`}
                onMouseEnter={() => setHover(2 * i - 1)}
                onFocus={() => setHover(2 * i - 1)}
                onBlur={() => setHover(null)}
                onClick={() => choose(2 * i - 1)}
              />
              <button
                type="button"
                className="half right"
                aria-label={`${i} stars`}
                onMouseEnter={() => setHover(2 * i)}
                onFocus={() => setHover(2 * i)}
                onBlur={() => setHover(null)}
                onClick={() => choose(2 * i)}
              />
            </>
          )}
        </span>
      ))}
    </span>
  );
}
