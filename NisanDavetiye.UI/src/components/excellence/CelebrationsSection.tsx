import type { Davetiye } from '../../types'
import {
  buildCalendarHref,
  handleCalendarClick,
  shouldOpenCalendarInSameTab,
} from '../../utils/calendar'
import { buildMapOpenUrl, shouldOpenMapInSameTab } from '../../utils/maps'
import { excellenceAssets } from '../../excellence/assets'
import { IconCalendar, IconMapPin } from '../Icons'
import { FadeIn } from './FadeIn'
import { ExcellenceSectionHeader } from './SectionHeader'

function hideOnError(e: React.SyntheticEvent<HTMLImageElement>) {
  e.currentTarget.style.display = 'none'
}

export function CelebrationsSection({ data }: { data: Davetiye }) {
  const date = new Date(data.etkinlikTarihi)
  const dateStr = date.toLocaleDateString('tr-TR', {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    year: 'numeric',
  })
  const time = date.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })
  const mapUrl = buildMapOpenUrl(data)
  const mapSameTab = shouldOpenMapInSameTab()
  const calendarHref = buildCalendarHref(data)
  const calendarSameTab = shouldOpenCalendarInSameTab()

  return (
    <section className="ex-section ex-celebrations">
      <div className="ex-celebrations__inner">
        <FadeIn>
          <ExcellenceSectionHeader
            title="Detaylar"
            titleScript
            subtitle=""
          />
        </FadeIn>

        <FadeIn delay={0.1}>
          <article className="ex-card">
              <p className="ex-card__eyebrow">Nişan Töreni</p>
              <h3 className="ex-card__title ex-card__title--display">{data.mekanAdi}</h3>
              {data.adres && <p className="ex-card__detail">{data.adres}</p>}
              <div className="ex-card__rule" aria-hidden />
              <p className="ex-card__date-line">{dateStr}</p>
              <p className="ex-card__time-line">Saat {time}</p>
              <div className="ex-card__actions">
                <a
                  href={mapUrl}
                  {...(mapSameTab ? {} : { target: '_blank', rel: 'noreferrer' })}
                  className="ex-btn ex-btn--link"
                >
                  <IconMapPin />
                  Haritada Aç
                </a>
                <a
                  href={calendarHref}
                  {...(calendarSameTab
                    ? { download: 'nis-davetiye.ics' }
                    : { target: '_blank', rel: 'noreferrer' })}
                  onClick={(e) => handleCalendarClick(data, e)}
                  className="ex-btn ex-btn--link"
                >
                  <IconCalendar />
                  Takvimde Aç
                </a>
              </div>
            </article>

            <div className="ex-celebrations__candles-wrap">
              <img
                src={excellenceAssets.candles}
                alt=""
                className="ex-celebrations__candles ornament-img"
                draggable={false}
                onError={hideOnError}
              />
            </div>
        </FadeIn>
      </div>
    </section>
  )
}
